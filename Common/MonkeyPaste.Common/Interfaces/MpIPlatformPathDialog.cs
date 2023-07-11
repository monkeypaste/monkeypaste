using System.Threading.Tasks;

namespace MonkeyPaste.Common {
    public interface MpIPlatformPathDialog {
        Task<string> ShowFileDialogAsync(string title = "", string initDir = "", object filters = null, bool resolveShortcutPath = false);
        Task<string> ShowFolderDialogAsync(string title = "", string initDir = "");
    }
}
