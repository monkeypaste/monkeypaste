using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Layout;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
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

        #endregion

        #region Statics
        public static string[] Args { get; set; } = new string[] { };

        private static App _instance;
        public static Window MainWindow {
            get {
                if (_instance == null) {
                    return null;
                }
                if (_instance.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop) {
                    return desktop.MainWindow as MpAvMainWindow;
                }
                return null;
            }
        }

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
            //DataContext = MpAvSystemTrayViewModel.Instance;
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
            ReportCommandLineArgs(Args);
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop) {
                desktop.Startup += Startup;
                desktop.Exit += Exit;

                var bootstrapper = new MpAvBootstrapperViewModel();
                await bootstrapper.InitAsync();
            } else if (ApplicationLifetime is ISingleViewApplicationLifetime mobile) {
                Dispatcher.UIThread.Post(async () => {
                    var bootstrapper = new MpAvBootstrapperViewModel();
                    await bootstrapper.InitAsync();
                });
                mobile.MainView = new MpAvMainView() {
                    HorizontalAlignment = HorizontalAlignment.Stretch,
                    VerticalAlignment = VerticalAlignment.Stretch,
                    DataContext = MpAvMainWindowViewModel.Instance
                };
            }

            //if (MpAvCefNetApplication.UseCefNet) {
            //    MpAvCefNetApplication.Init();
            //}


            //while (!MpBootstrapperViewModelBase.IsPlatformLoaded) {
            //    await Task.Delay(100);
            //}

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

        private void RootIcon_Clicked(object sender, EventArgs e) {
            MpAvMainWindowViewModel.Instance.IsAnyDialogOpen = DateTime.Now - MpAvShortcutCollectionViewModel.Instance.LastRightClickDateTime < TimeSpan.FromMilliseconds(1000);

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
