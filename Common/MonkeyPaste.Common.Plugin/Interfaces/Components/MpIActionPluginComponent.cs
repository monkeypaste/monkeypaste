using System.Windows.Input;

namespace MonkeyPaste.Common.Plugin {
    public interface MpIActionPluginComponent : MpIPluginComponentBase {
        ICommand PerformActionCommand { get; }

    }
}
