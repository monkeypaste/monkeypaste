using MonkeyPaste.Common;
using MonkeyPaste.Common.Plugin;
using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;

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
    }
}
