using MonkeyPaste.Common.Avalonia.Plugin;
using MonkeyPaste.Common.Plugin;

namespace MonkeyPaste.Avalonia {
    public interface MpIParameterHostViewModel : MpIViewModel {
        int IconId { get; }
        MpAvPluginFormat PluginFormat { get; }
        MpParameterHostBaseFormat ComponentFormat { get; }
        MpParameterHostBaseFormat BackupComponentFormat { get; }
        string PluginGuid { get; }
        MpIPluginComponentBase PluginComponent { get; }
    }
}
