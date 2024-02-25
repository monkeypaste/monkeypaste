using MonkeyPaste.Common;
using MonkeyPaste.Common.Plugin;
using MonkeyPaste.Common.Wpf;
using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;

namespace MonkeyPaste.Avalonia {
    public static class MpAvAppRestarter {

        public static async Task ShutdownWithRestartTaskAsync(string detail) {
            string launcher_path =
                Path.Combine(
                    Path.GetDirectoryName(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)),
                    "MonkeyPaste.Desktop.Launcher",
                    "MonkeyPaste.Desktop.Launcher.exe");
            Process process = Process.Start(launcher_path, App.RESTART_ARG);
            Mp.Services.ShutdownHelper.ShutdownApp(MpShutdownType.Restart, detail);

            //if (result == AppRestartFailureReason.NotInForeground ||
            //    result == AppRestartFailureReason.RestartPending ||
            //    result == AppRestartFailureReason.Other) {
            //    var result2 = await Mp.Services.NotificationBuilder.ShowNotificationAsync(
            //        notificationType: MpNotificationType.ModalShutdownLater,
            //        title: UiStrings.CommonRestartFailedTitle,
            //        body: UiStrings.CommonRestartFailedText,
            //        iconSourceObj: "ErrorImage");
            //    if(result2 == MpNotificationDialogResultType.Cancel) {
            //        return;
            //    }
            //}

            await Task.Delay(1);
            //RequestRestartAsync(true, detail).FireAndForgetSafeAsync();
        }

        private static async Task RequestRestartAsync(bool is_initial, string detail) {
            if (is_initial) {
                //Mp.Services.ShutdownHelper.ShutdownApp(MpShutdownType.Restart, detail);
            }

            WinApi.SetActiveWindow(MpAvWindowManager.PrimaryHandle);
            WinApi.SetForegroundWindow(MpAvWindowManager.PrimaryHandle);

            var result = await CoreApplication.RequestRestartAsync(App.RESTART_ARG);
            if (result == AppRestartFailureReason.RestartPending) {
                return;
            }
            MpConsole.WriteLine($"Restart failed. Reason: '{result}'");
            await Task.Delay(100);
            RequestRestartAsync(false, detail).FireAndForgetSafeAsync();
        }
    }
}
