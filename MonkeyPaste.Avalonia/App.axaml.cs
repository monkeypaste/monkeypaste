using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Avalonia;
using PropertyChanged;
using System;
using System.Diagnostics;
using System.Linq;

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
        public static string[] Args { get; set; }

        //public static Control MainWindowOrView {
        //    get {
        //        if (Lifetime is IClassicDesktopStyleApplicationLifetime cdsal) {
        //            return cdsal.MainWindow;
        //        }
        //        if (Lifetime is ISingleViewApplicationLifetime sval) {
        //            return sval.MainView;
        //        }
        //        return null;
        //    }
        //}

        private static App _instance;
        public static Window MainWindow {
            get {
                if (_instance == null) {
                    return null;
                }
                return _instance.GetMainWindow();
            }
            set {
                if (_instance == null) {
                    return;
                }
                _instance.SetMainWindow(value);
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
            if (ApplicationLifetime is IControlledApplicationLifetime lifetime) {

                lifetime.Startup += Startup;
                lifetime.Exit += Exit;

                ReportCommandLineArgs(Args);

                if (MpAvCefNetApplication.UseCefNet) {
                    MpAvCefNetApplication.InitCefNet();
                }

                var bootstrapper = new MpAvBootstrapperViewModel();
                await bootstrapper.InitAsync();

            } else {
                MpDebug.Break();
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
