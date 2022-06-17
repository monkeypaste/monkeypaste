using MonkeyPaste.Common;

namespace MonkeyPaste.Common.Plugin {
    public interface MpIClipboardPluginComponent : MpIPluginComponentBase {
        MpPortableDataObject GetClipboardData();
        void SetClipboardData(MpPortableDataObject pdo);
    }
}
