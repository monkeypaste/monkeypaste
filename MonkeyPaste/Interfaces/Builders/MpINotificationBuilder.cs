using MonkeyPaste.Common;
using MonkeyPaste.Common.Plugin;
using System;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MonkeyPaste {
    public interface MpINotificationBuilder {
        Task<string> ShowInputResultNotificationAsync(string title, string body, string currentInput = null, string placeholderText = null, object anchor = null, object iconResourceObj = null, object owner = null, char passwordChar = default, MpNotificationType ntfType = MpNotificationType.ModalTextBoxOkCancelMessageBox);
        Task<(string, bool)> ShowRememberableInputResultNotificationAsync(string title, string body, string currentInput = null, string placeholderText = null, object anchor = null, object iconResourceObj = null, object owner = null, char passwordChar = default, MpNotificationType ntfType = MpNotificationType.ModalRememberableTextBoxOkCancelMessageBox);
        Task<MpNotificationDialogResultType> ShowLoaderNotificationAsync(MpIProgressLoaderViewModel loader);
        Task ShowMessageAsync(MpNotificationType msgType, string title = "", object body = null, int maxShowTimeMs = MpNotificationFormat.MAX_MESSAGE_DISPLAY_MS, object iconSourceObj = null, object anchor = null);
        Task<MpNotificationDialogResultType> ShowNotificationAsync(MpNotificationType notificationType = MpNotificationType.None, string title = "", object body = null, int maxShowTimeMs = -1, Func<object, object> retryAction = null, object retryActionObj = null, object iconSourceObj = null, object anchor = null, ICommand fixCommand = null, object fixCommandArgs = null, MpIProgressLoaderViewModel loader = null, object owner = null, object otherArgs = null);
        Task<MpNotificationDialogResultType> ShowNotificationAsync(MpINotificationFormat inf);
    }
}