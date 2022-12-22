using System.Threading.Tasks;

namespace MonkeyPaste {
    public interface MpINativeMessageBox {
        Task<bool> ShowOkCancelMessageBoxAsync(string title, string message, object anchor = null, object iconResourceObj = null);

        Task<bool?> ShowYesNoCancelMessageBoxAsync(string title, string message, object anchor = null, object iconResourceObj = null);
    }
}
