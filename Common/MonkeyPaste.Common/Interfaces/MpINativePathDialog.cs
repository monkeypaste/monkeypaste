using System.Threading.Tasks;

namespace MonkeyPaste.Common {
    public interface MpINativePathDialog {
        Task<string> ShowFileDialogAsync(string title, string initDir, string[] filters);
        Task<string> ShowFolderDialogAsync(string title, string initDir);
    }
}
