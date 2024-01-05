using Avalonia.Controls;
using Avalonia.Media;
using MonkeyPaste.Common;
using System.Threading.Tasks;

namespace MonkeyPaste.Avalonia {
    public class MpAvMessageBox : MpIPlatformMessageBox {
        public async Task ShowRestartNowOrLaterMessageBoxAsync(
            string title,
            string message = default,
            object anchor = null,
            object iconResourceObj = null,
            object owner = null) {
            // NOTE only returns if later is chosen
            _ = await Mp.Services.NotificationBuilder.ShowNotificationAsync(
                                    notificationType: MpNotificationType.ModalRestartNowOrLater,
                                    title: title,
                                    body: message,
                                    iconSourceObj: iconResourceObj,
                                    anchor: anchor,
                                    owner: owner);
        }
        public async Task<bool> ShowRestartIgnoreCancelMessageBoxAsync(
            string title,
            string message = default,
            object anchor = null,
            object iconResourceObj = null,
            object owner = null) {
            // NOTE only returns if its ignored or canceled
            // returns true if ignored
            var result = await Mp.Services.NotificationBuilder.ShowNotificationAsync(
                                    notificationType: MpNotificationType.ModalRestartIgnore,
                                    title: title,
                                    body: message,
                                    iconSourceObj: iconResourceObj,
                                    anchor: anchor,
                                    owner: owner);
            return result == MpNotificationDialogResultType.Cancel;
        }
        public void ShowWebViewWindow(
            string title,
            string address,
            double width = 500,
            double height = 500,
            object owner = null,
            object dataContext = null,
            bool canResize = true,
            object iconResourceObj = null,
            MpThemeResourceKey background = MpThemeResourceKey.ThemeInteractiveBgColor,
            MpWindowType windowType = MpWindowType.Modal) {
            var w = new MpAvWindow() {
                Width = width,
                Height = height,
                CanResize = canResize,
                WindowType = windowType,
                DataContext = dataContext,
                Background = Mp.Services.PlatformResource.GetResource<IBrush>(background.ToString()),
                Title = title.ToWindowTitleText(),
                WindowStartupLocation =
                    owner == null ?
                        WindowStartupLocation.CenterScreen :
                        WindowStartupLocation.CenterOwner,
                Icon = MpAvIconSourceObjToBitmapConverter.Instance.Convert(
                    iconResourceObj == null ? "AppIcon" : iconResourceObj,
                    typeof(WindowIcon), null, null) as WindowIcon,
                Content = new MpAvWebPageView() {
                    Address = address
                },
            };
            w.Classes.Add("fadeIn");
            w.Show(owner as Window);
        }
        public async Task<bool> ShowProgressMessageBoxAsync(
            string title,
            string message,
            object anchor = null,
            object iconResourceObj = null,
            object owner = null,
            object iprog_and_or_cancel_token_arg = null) {
            // NOTE returns true if percent completes, false if canceled by user or token
            // NOTE2 only shows cancel if arg contains cancellation token
            var result = await Mp.Services.NotificationBuilder.ShowNotificationAsync(
                                    notificationType: MpNotificationType.ModalProgressCancelMessageBox,
                                    title: title,
                                    body: message,
                                    iconSourceObj: iconResourceObj,
                                    anchor: anchor,
                                    owner: owner,
                                    maxShowTimeMs: -1,
                                    otherArgs: iprog_and_or_cancel_token_arg);
            if (result == MpNotificationDialogResultType.Dismiss) {
                return true;
            }
            return false;
        }
        public async Task<bool> ShowBusyMessageBoxAsync(
            string title,
            string message,
            object anchor = null,
            object iconResourceObj = null,
            object owner = null,
            bool can_user_cancel = false,
            object cancel_token_arg = null) {
            // returns true if user cancels or ct is canceled
            var result = await Mp.Services.NotificationBuilder.ShowNotificationAsync(
                                    notificationType: MpNotificationType.ModalBusyMessageBox,
                                    title: title,
                                    body: message,
                                    iconSourceObj: iconResourceObj,
                                    anchor: anchor,
                                    owner: owner,
                                    maxShowTimeMs: -1,
                                    otherArgs: new object[] { cancel_token_arg, can_user_cancel });
            return result == MpNotificationDialogResultType.Cancel;
        }
        public async Task ShowOkMessageBoxAsync(
            string title,
            string message,
            object anchor = null,
            object iconResourceObj = null,
            object owner = null) {
            await Mp.Services.NotificationBuilder.ShowNotificationAsync(
                                    notificationType: MpNotificationType.ModalOkMessageBox,
                                    title: title,
                                    body: message,
                                    iconSourceObj: iconResourceObj,
                                    anchor: anchor,
                                    owner: owner);
        }
        public async Task<bool> ShowOkCancelMessageBoxAsync(
            string title,
            string message,
            object anchor = null,
            object iconResourceObj = null,
            object owner = null,
            MpNotificationType ntfType = MpNotificationType.ModalOkCancelMessageBox) {
            var result = await Mp.Services.NotificationBuilder.ShowNotificationAsync(
                                    notificationType: ntfType,
                                    title: title,
                                    body: message,
                                    iconSourceObj: iconResourceObj,
                                    anchor: anchor,
                                    owner: owner);

            if (result == MpNotificationDialogResultType.Ok) {
                return true;
            }

            if (result != MpNotificationDialogResultType.Cancel) {
                if (ntfType != MpNotificationType.ModalOkCancelMessageBox) {
                    // non-default ok/cancel box
                    if (result == MpNotificationDialogResultType.Dismiss ||
                        result == MpNotificationDialogResultType.DoNotShow) {
                        // user chose to ignore this confirm ie always confirm
                        return true;
                    }
                }
                // result type mismatch
                MpDebug.Break();
            }
            return false;
        }
        public async Task<bool> ShowYesNoMessageBoxAsync(
            string title,
            string message,
            object anchor = null,
            object iconResourceObj = null,
            object owner = null) {
            var result = await Mp.Services.NotificationBuilder.ShowNotificationAsync(
                                    notificationType: MpNotificationType.ModalYesNoMessageBox,
                                    title: title,
                                    body: message,
                                    iconSourceObj: iconResourceObj,
                                    anchor: anchor,
                                    owner: owner);

            return result == MpNotificationDialogResultType.Yes;
        }

        public async Task<bool?> ShowYesNoCancelMessageBoxAsync(
            string title,
            string message,
            object anchor = null,
            object iconResourceObj = null,
            object owner = null) {
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

        public async Task<string> ShowTextBoxMessageBoxAsync(
            string title,
            string message,
            string currentText = null,
            string placeholderText = null,
            object anchor = null,
            object iconResourceObj = null,
            object owner = null,
            char passwordChar = default,
            MpNotificationType ntfType = MpNotificationType.ModalTextBoxOkCancelMessageBox) {
            var result = await Mp.Services.NotificationBuilder.ShowInputResultNotificationAsync(
                                    title: title,
                                    body: message,
                                    currentInput: currentText,
                                    placeholderText: placeholderText,
                                    iconResourceObj: iconResourceObj,
                                    anchor: anchor,
                                    owner: owner,
                                    passwordChar: passwordChar,
                                    ntfType: ntfType);
            return result;
        }

        public async Task<(string, bool)> ShowRememberableTextBoxMessageBoxAsync(
            string title,
            string message,
            string currentText = null,
            string placeholderText = null,
            object anchor = null,
            object iconResourceObj = null,
            object owner = null,
            char passwordChar = default,
            MpNotificationType ntfType = MpNotificationType.ModalRememberableTextBoxOkCancelMessageBox) {
            var result = await Mp.Services.NotificationBuilder.ShowRememberableInputResultNotificationAsync(
                                    title: title,
                                    body: message,
                                    currentInput: currentText,
                                    placeholderText: placeholderText,
                                    iconResourceObj: iconResourceObj,
                                    anchor: anchor,
                                    owner: owner,
                                    passwordChar: passwordChar,
                                    ntfType: ntfType);
            return result;
        }
    }
}
