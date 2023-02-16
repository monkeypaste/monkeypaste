using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using PropertyChanged;
using System;
using System.Diagnostics;

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
        public static IClassicDesktopStyleApplicationLifetime Desktop { get; private set; }
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
        }
        #endregion

        #region Public Methods
        public override void Initialize() {
            AvaloniaXamlLoader.Load(this);
        }
        public override async void OnFrameworkInitializationCompleted() {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop) {
                Desktop = desktop;

                desktop.Startup += Startup;
                desktop.Exit += Exit;

                ReportCommandLineArgs(Args);

                if (MpAvCefNetApplication.UseCefNet) {
                    MpAvCefNetApplication.InitCefNet();
                }

                var bootstrapper = new MpAvBootstrapperViewModel();
                await bootstrapper.InitAsync();

            } else if (ApplicationLifetime is ISingleViewApplicationLifetime singleView) {
                Debugger.Break();
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
