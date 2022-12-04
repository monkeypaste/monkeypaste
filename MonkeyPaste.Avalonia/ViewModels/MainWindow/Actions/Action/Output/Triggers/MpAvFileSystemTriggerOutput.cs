using System.IO;

namespace MonkeyPaste.Avalonia {
    public class MpAvFileSystemTriggerOutput : MpAvActionOutput {
        public override object OutputData { get; }

        public WatcherChangeTypes FileSystemChangeType { get; set; }
        public override string ActionDescription => $"CopyItem({CopyItem.Id},{CopyItem.Title}) had file system change of type: {FileSystemChangeType}";
    }
}
