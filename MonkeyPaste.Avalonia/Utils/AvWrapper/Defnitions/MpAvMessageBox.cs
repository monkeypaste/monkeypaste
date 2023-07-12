using MonkeyPaste.Common;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace MonkeyPaste.Avalonia {
    public class MpAvMessageBox : MpIPlatformMessageBox {
        public async Task<bool> ShowCancelableProgressMessageBoxAsync(
            string title, string message, object anchor = null, object iconResourceObj = null, object owner = null, object iprog_and_or_cancel_token_arg = null) {
            // NOTE returns true if percent completes, false if canceled by user or token
            var result = await Mp.Services.NotificationBuilder.ShowNotificationAsync(
                                    notificationType: MpNotificationType.ModalProgressCancelMessageBox,
                                    title: title,
                                    body: message,
                                    iconSourceObj: iconResourceObj,
                                    anchor: anchor,
                                    owner: owner,
                                    otherArgs: iprog_and_or_cancel_token_arg);
            if (result == MpNotificationDialogResultType.Dismiss) {
                return true;
            }
            return false;
        }
        public async Task ShowOkMessageBoxAsync(string title, string message, object anchor = null, object iconResourceObj = null, object owner = null) {
            await Mp.Services.NotificationBuilder.ShowNotificationAsync(
                                    notificationType: MpNotificationType.ModalOkMessageBox,
                                    title: title,
                                    body: message,
                                    iconSourceObj: iconResourceObj,
                                    anchor: anchor,
                                    owner: owner);
        }
        public async Task<bool> ShowOkCancelMessageBoxAsync(string title, string message, object anchor = null, object iconResourceObj = null, object owner = null) {
            var result = await Mp.Services.NotificationBuilder.ShowNotificationAsync(
                                    notificationType: MpNotificationType.ModalOkCancelMessageBox,
                                    title: title,
                                    body: message,
                                    iconSourceObj: iconResourceObj,
                                    anchor: anchor,
                                    owner: owner);

            if (result == MpNotificationDialogResultType.Ok) {
                return true;
            }

            if (result != MpNotificationDialogResultType.Cancel) {
                // result type mismatch
                MpDebug.Break();
            }
            return false;
        }
        public async Task<bool> ShowYesNoMessageBoxAsync(string title, string message, object anchor = null, object iconResourceObj = null, object owner = null) {
            var result = await Mp.Services.NotificationBuilder.ShowNotificationAsync(
                                    notificationType: MpNotificationType.ModalYesNoMessageBox,
                                    title: title,
                                    body: message,
                                    iconSourceObj: iconResourceObj,
                                    anchor: anchor,
                                    owner: owner);

            return result == MpNotificationDialogResultType.Yes;
        }

        public async Task<bool?> ShowYesNoCancelMessageBoxAsync(string title, string message, object anchor = null, object iconResourceObj = null, object owner = null) {
            var result = await Mp.Services.NotificationBuilder.ShowNotificationAsync(
                                    notificationType: MpNotificationType.ModalYesNoCancelMessageBox,
                                    title: title,
                                    body: message,
                                    iconSourceObj: iconResourceObj,
                                    anchor: anchor,
                                    owner: owner);

            if (result == MpNotificationDialogResultType.Yes) {
                return true;
            }
            if (result == MpNotificationDialogResultType.No) {
                return false;
            }

            if (result != MpNotificationDialogResultType.Cancel) {
                // result type mismatch
                MpDebug.Break();
            }
            return null;
        }

        public async Task<string> ShowTextBoxMessageBoxAsync(string title, string message, string currentText = null, string placeholderText = null, object anchor = null, object iconResourceObj = null, object owner = null) {
            var result = await Mp.Services.NotificationBuilder.ShowInputResultNotificationAsync(
                                    title: title,
                                    body: message,
                                    currentInput: currentText,
                                    placeholderText: placeholderText,
                                    iconResourceObj: iconResourceObj,
                                    anchor: anchor,
                                    owner: owner);
            return result;
        }
    }
}
