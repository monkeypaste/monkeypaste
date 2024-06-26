using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Layout;
using Avalonia.Logging;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Threading;
#if SUGAR_WV
using AvaloniaWebView;
#endif
using MonkeyPaste.Common;
using MonkeyPaste.Common.Plugin;
using PropertyChanged;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;

namespace MonkeyPaste.Avalonia {
    [DoNotNotify]
    public partial class App : Application {
        #region Private Variable
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
            if (args.Contains(WAIT_FOR_DEBUG_ARG)) {
                Console.WriteLine("Attach debugger and use 'Set next statement'");
                while (true) {
                    Thread.Sleep(100);
                    if (Debugger.IsAttached) {
                        if(args.Contains(BREAK_ON_ATTACH_ARG)) {
                            Debugger.Break();
                        }
                        break;
                    }
                }
            }
        }
        public static void SetPrimaryView(Control c) {
            if (_instance == null ||
                _instance.ApplicationLifetime is not ISingleViewApplicationLifetime sval ||
                sval.MainView is not Border b) {
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
#if WINDOWED
                MpAvRootWindow.Instance.Show();
#endif
            } else if (ApplicationLifetime is ISingleViewApplicationLifetime mobile) {
                
                mobile.MainView = new Border() {
                    //Margin = new Thickness(0, 24, 0, 0),
                    HorizontalAlignment = HorizontalAlignment.Stretch,
                    VerticalAlignment = VerticalAlignment.Stretch,
                    Background = Brushes.Silver,
                };

                var loader = new MpAvLoaderViewModel(is_login_load);
                await loader.CreatePlatformAsync(startup_datetime);

                Dispatcher.UIThread.Post(async () => {
                    if (MpDeviceWrapper.Instance != null) {
                        await MpDeviceWrapper.Instance.InitAsync(null);
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

    internal class MpAvLogSink : ILogSink {
        private ILogSink _defSink;

        private (LogEventLevel, string)[] _disabledLogs = {
            (LogEventLevel.Warning,LogArea.Binding)
        };
        public static void Init() {
            _ = new MpAvLogSink();
        }
        private MpAvLogSink() {
            if (Logger.Sink != this) {
                _defSink = Logger.Sink;
            }

            Logger.Sink = this;
        }

        public bool IsEnabled(LogEventLevel level, string area) {
            if (!_defSink.IsEnabled(level, area)) {
                return false;
            }
            return _disabledLogs.All(x => x.Item1 != level && x.Item2 != area);
        }

        public void Log(LogEventLevel level, string area, object source, string messageTemplate) {
            _defSink.Log(level, area, source, messageTemplate);
        }

        public void Log(LogEventLevel level, string area, object source, string messageTemplate, params object[] propertyValues) {
            _defSink.Log(level, area, source, messageTemplate, propertyValues);
        }
    }
}
