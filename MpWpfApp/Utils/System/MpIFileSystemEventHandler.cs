using System.IO;

namespace MpWpfApp {
    public interface MpIFileSystemEventHandler {
        bool IncludeSubdirectories { get; }
        void OnFileSystemItemChanged(object sender, FileSystemEventArgs e);
    }
}
