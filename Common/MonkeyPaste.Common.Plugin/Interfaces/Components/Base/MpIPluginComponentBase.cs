namespace MonkeyPaste.Common.Plugin {
    public interface MpIPluginComponentBase {
    }
    public interface MpIUnloadPluginComponent : MpIPluginComponentBase {
        void Unload();
    }
}
