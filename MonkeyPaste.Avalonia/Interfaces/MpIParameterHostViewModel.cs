using MonkeyPaste.Common.Plugin;

namespace MonkeyPaste.Avalonia {
    public interface MpIParameterHostViewModel : MpIViewModel {
        int IconId { get; }
        MpPluginWrapper PluginFormat { get; }
        MpParameterHostBaseFormat ComponentFormat { get; }
        MpParameterHostBaseFormat BackupComponentFormat { get; }
        string PluginGuid { get; }
        MpIPluginComponentBase PluginComponent { get; }
    }
}
