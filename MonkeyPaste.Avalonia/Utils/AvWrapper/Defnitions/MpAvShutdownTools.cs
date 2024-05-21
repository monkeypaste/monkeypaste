using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Plugin;

namespace MonkeyPaste.Avalonia {
    public class MpAvShutdownTools : MpIShutdownTools {
        //private int _shutdownCount = 0;
        private bool _isShuttingDown;
//        private bool _isShuttingDown =>
//#if LINUX
//            _shutdownCount > 2;
//#else
//            _shutdownCount >= 1;
//#endif
        public void ShutdownApp(MpShutdownType code, string detail) {
            //_shutdownCount++;
            if (_isShuttingDown) {
                return;
            }
            _isShuttingDown = true;
            MpConsole.WriteLine($"App shutdown called Code: '{code}' Detail: '{detail.ToStringOrEmpty("NULL")}'");
            //MpConsole.ShutdownLog();
            if (App.Instance != null &&
                App.Instance.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime lifetime) {
                MpAvSystemTray.ShutdownTray(App.Instance);
                lifetime.Shutdown();

                //MpAvWindowManager.CloseAll();
//#if CEFNET_WV
//                MpAvCefNetApplication.ShutdownCefNet();
//#endif
//                var app = Application.Current;
//#if LINUX
//                // BUG sys tray doesn't close when app does on linux
//                // this DOES close it but throws segmentation fault
//                MpAvSystemTray.ShutdownTray(app);
//#endif
//                lifetime.Shutdown();
//                bool success = true;// lifetime.TryShutdown();
//                MpConsole.WriteLine($"Lifetime shutdown: {success.ToTestResultLabel()}");
//#if LINUX
//                ShutdownApp(code, "Ensuring shutdown...");
//#endif
            }
        }
    }
}

