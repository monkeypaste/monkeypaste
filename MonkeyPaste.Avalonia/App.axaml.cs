using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Layout;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Threading;
#if SUGAR_WV
using AvaloniaWebView;
#endif
using MonkeyPaste.Common;
using MonkeyPaste.Common.Avalonia;
using MonkeyPaste.Common.Plugin;
using PropertyChanged;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Input;

#if ENABLE_XAML_HOT_RELOAD
using HotAvalonia; 
#endif

namespace MonkeyPaste.Avalonia {
    [DoNotNotify]
    public partial class App : Application {
        #region Private Variable

        private bool is_xaml_hot_reload_enabled =
#if ENABLE_XAML_HOT_RELOAD
            true;
#else
            false;
#endif

        #endregion

        #region Constants
        public const string MULTI_TOUCH_ARG = "--multitouch";
        public const string LOGIN_LOAD_ARG = "--loginload";
        public const string RESTART_ARG = "--restarted";
        public const string TRACE_ARG = "--trace";
        public const string WAIT_FOR_DEBUG_ARG = "--wait-for-attach";
        public const string BREAK_ON_ATTACH_ARG = "--break-on-attach";
        public const string NO_ATTACH_ARG = "--no-attach";

        #endregion

        #region Statics
        private static string _instanceGuid;
        public static string InstanceGuid => _instanceGuid ?? (_instanceGuid = System.Guid.NewGuid().ToString());
        public static string[] Args { get; set; } = new string[] { };

        private static App _instance;
        public static App Instance =>
            _instance;

        public static Control MainView {
            get {
                if (_instance == null) {
                    return null;
                }

                if (_instance.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop) {
                    if(MpAvThemeViewModel.Instance.IsWindowed) {
                        return desktop.MainWindow.Content as Control;
                    }
                    return desktop.MainWindow;
                }
                if (_instance.ApplicationLifetime is ISingleViewApplicationLifetime mobile) {
                    return mobile.MainView;
                }
                return null;
            }
        }
        public static Control PrimaryView =>
#if MOBILE
            MainView == null ? null : (MainView as Border).Child; 
#else
            MainView;
#endif

        public static void WaitForDebug(object[] args) {
            if (!args.Contains(WAIT_FOR_DEBUG_ARG)) {
                return;
            }
            Console.WriteLine("Attach debugger and use 'Set next statement'");
            while (true) {
                Thread.Sleep(100);
                if (Debugger.IsAttached) {
                    if (args.Contains(BREAK_ON_ATTACH_ARG)) {
                        Debugger.Break();
                    }
                    break;
                }
            }
        }
        public static void SetPrimaryView(Control c) {
            if (_instance == null ||
                _instance.ApplicationLifetime is not ISingleViewApplicationLifetime sval ||
                sval.MainView is not Border b) {
                if(_instance != null && _instance.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime lt) {
                    lt.MainWindow.DataContext = c.DataContext;
                    lt.MainWindow.Content = c;
                }
                return;
            }
            b.Child = c;
        }

        public static bool HasStartupArg(string arg) {
            return Args.Any(x => x.ToLowerInvariant() == arg.ToLowerInvariant());
        }
        #endregion

        #region Interfaces

        #endregion

        #region Properties
        #endregion

        #region Events
#if CEFNET_WV
        public static event EventHandler FrameworkInitialized;
        public static event EventHandler FrameworkShutdown;
#endif
#endregion

        #region Constructors
        public App() {
            if (_instance != null) {
                MpDebug.Break();
            }
            _instance = this;
        }

        #endregion

        #region Public Methods
        public override void Initialize() {
#if ENABLE_XAML_HOT_RELOAD
            this.EnableHotReload(); 
#endif
            AvaloniaXamlLoader.Load(this);
        }
#if SUGAR_WV
        public override void RegisterServices() {
            base.RegisterServices();
            AvaloniaWebViewBuilder.Initialize((config) => {
                MpAvWebView.ConfigureWebViewCreationProperties(config);
            });
        }

#endif

        public override async void OnFrameworkInitializationCompleted() {
            DateTime startup_datetime = DateTime.Now;
#if DESKTOP
            MpConsole.Init(new MpAvPlatformInfo_desktop().LogPath, Debugger.IsAttached || HasStartupArg(TRACE_ARG));
            MpAvLogSink.Init();
#endif

            ReportCommandLineArgs(Args);
            bool is_login_load = HasStartupArg(LOGIN_LOAD_ARG);

            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop) {
#if CEFNET_WV
                desktop.Startup += Startup;
                desktop.Exit += Exit;
#endif
                desktop.ShutdownMode = ShutdownMode.OnExplicitShutdown;

                desktop.ShutdownRequested += Desktop_ShutdownRequested;

                var loader = new MpAvLoaderViewModel(is_login_load);
                await loader.CreatePlatformAsync(startup_datetime);
                await loader.InitAsync();
            } else if (ApplicationLifetime is ISingleViewApplicationLifetime mobile) {
                
                mobile.MainView = new Border() {
                    //Margin = new Thickness(0, 24, 0, 0),
                    HorizontalAlignment = HorizontalAlignment.Stretch,
                    VerticalAlignment = VerticalAlignment.Stretch,
                    Background = Brushes.Silver,
                };
                if(mobile.MainView is Border b) {
                    b.EffectiveViewportChanged += B_EffectiveViewportChanged;
                }

                var loader = new MpAvLoaderViewModel(is_login_load);
                await loader.CreatePlatformAsync(startup_datetime);

                Dispatcher.UIThread.Post(async () => {
                    if (MpAvDeviceWrapper.Instance != null) {
                        await MpAvDeviceWrapper.Instance.InitAsync(null);
                    }
                    MpAvPrefViewModel.Instance.IsRichHtmlContentEnabled = false;
                    await loader.InitAsync();
                });
            }

            base.OnFrameworkInitializationCompleted();

#if DEBUG && DESKTOP
            this.AttachDevTools(MpAvWindow.DefaultDevToolOptions);
#endif
        }

        private void B_EffectiveViewportChanged(object sender, EffectiveViewportChangedEventArgs e) {
            // measuring only seems wrong on android and ios would need to recalculate extents
#if ANDROID
            if (sender is not Control c) {
                return;
            }
            MpConsole.WriteLine($"Screen: {MpAvDeviceWrapper.Instance.ScreenInfoCollection.Primary}");
            MpConsole.WriteLine($"MainView: {c.Bounds}");
            MpAvDeviceWrapper.Instance.ScreenInfoCollection.Primary.Bounds = c.Bounds.ToPortableRect();
            MpAvDeviceWrapper.Instance.ScreenInfoCollection.Primary.WorkingArea = c.Bounds.ToPortableRect(); 
#endif
        }

        private void Desktop_ShutdownRequested(object sender, ShutdownRequestedEventArgs e) {
            Mp.Services.ShutdownHelper.ShutdownApp(MpShutdownType.FrameworkExit, "ShutdownRequested triggered");
        }
        #endregion

        #region Protected Methods
        #endregion

        #region Private Methods

#if CEFNET_WV
        private void Startup(object sender, ControlledApplicationLifetimeStartupEventArgs e) {
            FrameworkInitialized?.Invoke(this, EventArgs.Empty);
        }

        private void Exit(object sender, ControlledApplicationLifetimeExitEventArgs e) {
            FrameworkShutdown?.Invoke(this, EventArgs.Empty);
        }
#endif

        private void ReportCommandLineArgs(string[] args) {
            MpConsole.WriteLine("Program args: ");
            MpConsole.WriteLine(string.Join(Environment.NewLine, args));
        }

        #endregion

        #region Commands
        public ICommand ToggleXamlHotReloadCommand => new MpCommand(
            () => {
#if ENABLE_XAML_HOT_RELOAD
                if(is_xaml_hot_reload_enabled) {
                    Application.Current.DisableHotReload();
                } else {
                    Application.Current.EnableHotReload();
                }
                is_xaml_hot_reload_enabled = !is_xaml_hot_reload_enabled;
                string msg = $"XAML Hot Reload: {(is_xaml_hot_reload_enabled ? "ENABLED" : "DISABLED")}";
                MpConsole.WriteLine(msg);

                Mp.Services.NotificationBuilder.ShowMessageAsync(MpNotificationType.Message,
                    title: "XAML Hot Reload Changed",
                    body: msg).FireAndForgetSafeAsync();
#endif
            });

        #endregion

#if WINDOWS
        static FileStream? _lockFile;
        private static bool EnableSingleInstance() {
            var dir = MpPlatformHelpers.GetStorageDir();
            Directory.CreateDirectory(dir);
            try {
                _lockFile = File.Open(Path.Combine(dir, ".lock"), FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None);
                _lockFile.ReadByte();
                _lockFile.Write(new byte[] { 1 }, 0, 1);
                _lockFile.Lock(0, _lockFile.Length);
                return true;
            }
            catch {
                return false;
            }
        }
#endif
    }
}
