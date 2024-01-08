using MonkeyPaste.Common.Plugin;

namespace MonkeyPaste.Avalonia {
    public interface MpIParameterHostViewModel : MpIViewModel {
        int IconId { get; }
        MpRuntimePlugin PluginFormat { get; }
        MpPresetParamaterHostBase ComponentFormat { get; }
        MpPresetParamaterHostBase BackupComponentFormat { get; }
        string PluginGuid { get; }
    }
}
