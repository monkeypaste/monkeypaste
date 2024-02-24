using Avalonia.Controls;
using System;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.UI.Popups;

namespace MonkeyPaste.Avalonia {
    public static class MpAvAppRestarter {

        public static async Task ShutdownWithRestartTaskAsync(string detail) {
            if (MpAvMainView.Instance is not { } mv ||
                TopLevel.GetTopLevel(mv) is not MpAvWindow mw) {
                return;
            }
            mw.Activate();
            var result = await CoreApplication.RequestRestartAsync(App.RESTART_ARG);

            if (result == AppRestartFailureReason.NotInForeground ||
                result == AppRestartFailureReason.RestartPending ||
                result == AppRestartFailureReason.Other) {
                var msgBox = new MessageDialog("Restart Failed", result.ToString());
                await msgBox.ShowAsync();
            } else {
                Mp.Services.ShutdownHelper.ShutdownApp(MpShutdownType.Restart, detail);
            }
        }
    }
}
