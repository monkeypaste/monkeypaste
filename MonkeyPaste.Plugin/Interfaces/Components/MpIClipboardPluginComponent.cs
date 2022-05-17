
namespace MonkeyPaste.Plugin {
    public interface MpIClipboardPluginComponent : MpIPluginComponentBase {
        MpPortableDataObject HandleDataObject(MpPortableDataObject pdo);
    }
}
