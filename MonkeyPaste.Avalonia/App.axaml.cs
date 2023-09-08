using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Layout;
using Avalonia.Markup.Xaml;
using MonkeyPaste.Common;
using PropertyChanged;
using System;
using System.IO;
using System.Linq;
#if PLAT_WV
using AvaloniaWebView;
using WebViewCore.Configurations;
#endif

namespace MonkeyPaste.Avalonia {
    [DoNotNotify]
    public partial class App : Application, MpIShutdownTools {
        #region Private Variable
        private bool _isShuttingDown = false;
        #endregion

        #region Constants
        public const string RESET_DATA_ARG = "resetdata";
        public const string BACKUP_DATA_ARG = "backupdata";
        public const string MULTI_TOUCH_ARG = "multitouch";
        public const string LOGIN_LOAD_ARG = "loginload";

        #endregion

        #region Statics
        public static string[] Args { get; set; } = new string[] { };

        private static App _instance;
        public static App Instance =>
            _instance;

        public static MpIMainView MainView {
            get {
                if (_instance == null) {
                    return null;
                }

                if (_instance.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop) {
                    return desktop.MainWindow as MpAvMainWindow;
                }
                if (_instance.ApplicationLifetime is ISingleViewApplicationLifetime mobile) {
                    return mobile.MainView as MpAvMainView;
                }
                return null;
            }
        }

        public static bool HasStartupArg(string arg) {
            return Args.Any(x => x.ToLower() == arg.ToLower());
        }
        #endregion

        #region Interfaces

        #region MpIShutdownTools Implementation
        void MpIShutdownTools.ShutdownApp(object args) {
            if (_isShuttingDown) {
                return;
            }
            _isShuttingDown = true;
            MpConsole.WriteLine($"App shutdown called. Args: '{args.ToStringOrEmpty("NULL")}'");
#if DESKTOP

            MpAvCefNetApplication.ShutdownCefNet();
#endif
            MpConsole.ShutdownLog();
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime lifetime) {
                lifetime.Shutdown();
            }
        }

        bool MpIShutdownTools.WasShutdownSignaled =>
            _isShuttingDown;
        #endregion
        #endregion

        #region Properties
        #endregion

        #region Events
        public static event EventHandler FrameworkInitialized;
        public static event EventHandler FrameworkShutdown;
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
#if PLAT_WV
        public override void RegisterServices() {
            base.RegisterServices();

            // if you use only WebView
                AvaloniaWebViewBuilder.Initialize(
                    _ => new WebViewCreationProperties() {
                        AdditionalBrowserArguments = "--process-per-site"
                    }); 
        }
#endif
        public override void Initialize() {
            AvaloniaXamlLoader.Load(this);
        }
        public override async void OnFrameworkInitializationCompleted() {
            DateTime startup_datetime = DateTime.Now;


            ReportCommandLineArgs(Args);
            bool is_login_load = HasStartupArg(LOGIN_LOAD_ARG);

            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop) {
                desktop.Startup += Startup;
                desktop.Exit += Exit;

                var loader = new MpAvLoaderViewModel(is_login_load);
                await loader.CreatePlatformAsync(startup_datetime);
                await loader.InitAsync();
            } else if (ApplicationLifetime is ISingleViewApplicationLifetime mobile) {
                if (MpDeviceWrapper.Instance != null) {
                    await MpDeviceWrapper.Instance.InitAsync(null);
                }
                var loader = new MpAvLoaderViewModel(is_login_load);
                await loader.CreatePlatformAsync(startup_datetime);
                loader.InitAsync().FireAndForgetSafeAsync();

                mobile.MainView = new MpAvMainView() {
                    HorizontalAlignment = HorizontalAlignment.Stretch,
                    VerticalAlignment = VerticalAlignment.Stretch,
                    DataContext = MpAvMainWindowViewModel.Instance
                };
            }

            base.OnFrameworkInitializationCompleted();

            //#if DEBUG
            //            this.AttachDevTools(MpAvWindow.DefaultDevToolOptions);
            //#endif
        }
        #endregion

        #region Protected Methods
        #endregion

        #region Private Methods

        private void Startup(object sender, ControlledApplicationLifetimeStartupEventArgs e) {
            FrameworkInitialized?.Invoke(this, EventArgs.Empty);
        }

        private void Exit(object sender, ControlledApplicationLifetimeExitEventArgs e) {
            FrameworkShutdown?.Invoke(this, EventArgs.Empty);
        }

        private void ReportCommandLineArgs(string[] args) {
            MpConsole.WriteLine("Program args: ");
            MpConsole.WriteLine(string.Join(Environment.NewLine, args));
        }

        #endregion

        #region Commands
        #endregion

        static FileStream? _lockFile;
        private static bool EnableSingleInstance() {


            string app_dir =
#if DEBUG
                "MonkeyPaste_DEBUG";
#else
                "MonkeyPaste";
#endif
            var dir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), app_dir);
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
    }
}
