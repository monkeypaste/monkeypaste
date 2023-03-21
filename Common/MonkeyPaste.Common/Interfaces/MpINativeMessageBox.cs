using System.Threading.Tasks;

namespace MonkeyPaste.Common {
    public interface MpINativeMessageBox {
        Task ShowOkMessageBoxAsync(string title, string message, object anchor = null, object iconResourceObj = null);
        Task<bool> ShowOkCancelMessageBoxAsync(string title, string message, object anchor = null, object iconResourceObj = null);

        Task<bool?> ShowYesNoCancelMessageBoxAsync(string title, string message, object anchor = null, object iconResourceObj = null);
        Task<string> ShowTextBoxMessageBoxAsync(string title, string message, string currentText = null, string placeholderText = null, object anchor = null, object iconResourceObj = null);
    }
}
