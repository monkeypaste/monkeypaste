using System.Windows;
using Hardcodet.Wpf.TaskbarNotification;

namespace MpWpfApp {
    /// <summary>
    /// Simple application. Check the XAML for comments.
    /// </summary>
    public partial class App : Application {
        private TaskbarIcon trayIcon;

        protected override void OnStartup(StartupEventArgs e) {
            base.OnStartup(e);

            //create the notifyicon (it's a resource declared in NotifyIconResources.xaml
            trayIcon = (TaskbarIcon)FindResource("MpTrayIcon");
        }

        protected override void OnExit(ExitEventArgs e) {
            trayIcon.Dispose(); //the icon would clean up automatically, but this is cleaner
            base.OnExit(e);
        }
    }
}
