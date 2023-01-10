using MonkeyPaste.Common.Plugin;

namespace MonkeyPaste {
    public interface MpIParameterHostViewModel : MpIViewModel {
        int IconId { get; }
        MpPluginFormat PluginFormat { get; }
        MpParameterHostBaseFormat ComponentFormat { get; }
        MpParameterHostBaseFormat BackupComponentFormat { get; }
        string PluginGuid { get; }
        MpIPluginComponentBase PluginComponent { get; }
    }
}
