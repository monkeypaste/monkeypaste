using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Layout;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Styling;
using Avalonia.Threading;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Avalonia;
using PropertyChanged;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace MonkeyPaste.Avalonia {
    [DoNotNotify]
    public partial class App : Application {
        #region Private Variable
        #endregion

        #region Constants
        public const string RESET_DATA_ARG = "resetdata";
        public const string BACKUP_DATA_ARG = "backupdata";
        public const string MULTI_TOUCH_ARG = "multitouch";

        #endregion

        #region Statics
        public static string[] Args { get; set; } = new string[] { };

        private static App _instance;

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
        #endregion

        #region Interfaces
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
        public override void Initialize() {
            AvaloniaXamlLoader.Load(this);
        }
        public override async void OnFrameworkInitializationCompleted() {
            DateTime startup_datetime = DateTime.Now;

            ReportCommandLineArgs(Args);
            //RequestedThemeVariant = ThemeVariant.Light;

            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop) {
                desktop.Startup += Startup;
                desktop.Exit += Exit;

                var bootstrapper = new MpAvLoaderViewModel();
                await bootstrapper.CreatePlatformAsync(startup_datetime);
                await bootstrapper.InitAsync();
            } else if (ApplicationLifetime is ISingleViewApplicationLifetime mobile) {
                if (MpDeviceWrapper.Instance != null) {
                    await MpDeviceWrapper.Instance.InitAsync(null);
                }
                var bootstrapper = new MpAvLoaderViewModel();
                await bootstrapper.CreatePlatformAsync(startup_datetime);
                bootstrapper.InitAsync().FireAndForgetSafeAsync();

                mobile.MainView = new MpAvMainView() {
                    HorizontalAlignment = HorizontalAlignment.Stretch,
                    VerticalAlignment = VerticalAlignment.Stretch,
                    DataContext = MpAvMainWindowViewModel.Instance
                };
            }

            base.OnFrameworkInitializationCompleted();
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
            Console.WriteLine("Program args: ");
            Console.Write(string.Join(Environment.NewLine, args));
        }
        #endregion

        #region Commands
        #endregion
    }
}
