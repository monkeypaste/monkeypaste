namespace MonkeyPaste.Common.Plugin {
    public interface MpIPluginHost  {
        int IconId { get; }
        MpPluginFormat PluginFormat { get; }
        MpPluginComponentBaseFormat ComponentFormat { get; }
        string PluginGuid { get; }
        MpIPluginComponentBase PluginComponent { get; }
    }
}
