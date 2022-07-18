using System.IO;

namespace MonkeyPaste.Avalonia {
    public interface MpIFileSystemEventHandler {
        bool IncludeSubdirectories { get; }
        void OnFileSystemItemChanged(object sender, FileSystemEventArgs e);
    }
}
