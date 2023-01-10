using System.Windows.Input;

namespace MonkeyPaste.Common.Plugin {
    public interface MpITriggerPluginComponent : MpIPluginComponentBase {
        ICommand PerformActionCommand { get; }

    }
}
