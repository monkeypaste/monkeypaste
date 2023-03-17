using System.Threading.Tasks;

namespace MonkeyPaste.Common {
    public interface MpINativePathDialog {
        Task<string> ShowFileDialogAsync(string title = "", string initDir = "", object filters = null, bool resolveShortcutPath = false);
        Task<string> ShowFolderDialogAsync(string title = "", string initDir = "");
    }
}
