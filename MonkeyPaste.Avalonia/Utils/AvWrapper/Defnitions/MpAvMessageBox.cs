using MonkeyPaste.Common;
using System.Diagnostics;
using System.Threading.Tasks;

namespace MonkeyPaste.Avalonia {
    public class MpAvMessageBox : MpINativeMessageBox {
        private object result;
        public async Task<bool> ShowOkCancelMessageBoxAsync(string title, string message, object anchor = null, object iconResourceObj = null) {
            var result = await MpNotificationBuilder.ShowNotificationAsync(
                                    notificationType: MpNotificationType.ModalOkCancelMessageBox,
                                    title: title,
                                    body: message,
                                    iconSourceObj: iconResourceObj,
                                    anchor: anchor);

            if (result == MpNotificationDialogResultType.Ok) {
                return true;
            }

            if (result != MpNotificationDialogResultType.Cancel) {
                // result type mismatch
                Debugger.Break();
            }
            return false;
        }

        public async Task<bool?> ShowYesNoCancelMessageBoxAsync(string title, string message, object anchor = null, object iconResourceObj = null) {
            var result = await MpNotificationBuilder.ShowNotificationAsync(
                                    notificationType: MpNotificationType.ModalYesNoCancelMessageBox,
                                    title: title,
                                    body: message,
                                    iconSourceObj: iconResourceObj,
                                    anchor: anchor);

            if (result == MpNotificationDialogResultType.Yes) {
                return true;
            }
            if (result == MpNotificationDialogResultType.No) {
                return false;
            }

            if (result != MpNotificationDialogResultType.Cancel) {
                // result type mismatch
                Debugger.Break();
            }
            return null;
        }

        public async Task<string> ShowTextBoxMessageBoxAsync(string title, string message, string currentText = null, string placeholderText = null, object anchor = null, object iconResourceObj = null) {
            var result = await MpNotificationBuilder.ShowInputResultNotificationAsync(
                                    title: title,
                                    body: message,
                                    currentInput: currentText,
                                    placeholderText: placeholderText,
                                    iconResourceObj: iconResourceObj,
                                    anchor: anchor);
            return result;
        }
    }
}
