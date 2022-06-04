using MonkeyPaste.Common;

namespace MonkeyPaste.Common.Plugin {
    public interface MpIClipboardPluginComponent : MpIPluginComponentBase {
        MpPortableDataObject HandleDataObject(MpPortableDataObject pdo);
    }
}
