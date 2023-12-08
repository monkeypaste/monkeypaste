using System.Threading.Tasks;

namespace MonkeyPaste.Common {
    public interface MpIPlatformMessageBox {
        Task ShowOkMessageBoxAsync(
            string title,
            string message,
            object anchor = null,
            object iconResourceObj = null,
            object owner = null);
        Task<bool> ShowProgressMessageBoxAsync(
            string title,
            string message = default,
            object anchor = null,
            object iconResourceObj = null,
            object owner = null,
            object iprog_and_or_cancel_token_arg = default);

        Task ShowBusyMessageBoxAsync(
            string title,
            string message = default,
            object anchor = null,
            object iconResourceObj = null,
            object owner = null,
            object cancel_token_arg = default);
        Task<bool> ShowOkCancelMessageBoxAsync(
            string title,
            string message,
            object anchor = null,
            object iconResourceObj = null,
            object owner = null,
            MpNotificationType ntfType = MpNotificationType.ModalOkCancelMessageBox);
        Task<bool?> ShowYesNoCancelMessageBoxAsync(
            string title,
            string message,
            object anchor = null,
            object iconResourceObj = null,
            object owner = null);
        Task<bool> ShowYesNoMessageBoxAsync(
            string title,
            string message,
            object anchor = null,
            object iconResourceObj = null,
            object owner = null);
        Task<string> ShowTextBoxMessageBoxAsync(
            string title,
            string message,
            string currentText = null,
            string placeholderText = null,
            object anchor = null,
            object iconResourceObj = null,
            object owner = null,
            char passwordChar = default,
            MpNotificationType ntfType = MpNotificationType.ModalTextBoxOkCancelMessageBox);

        Task<(string, bool)> ShowRememberableTextBoxMessageBoxAsync(
            string title,
            string message,
            string currentText = null,
            string placeholderText = null,
            object anchor = null,
            object iconResourceObj = null,
            object owner = null,
            char passwordChar = default,
            MpNotificationType ntfType = MpNotificationType.ModalRememberableTextBoxOkCancelMessageBox);
    }
}
