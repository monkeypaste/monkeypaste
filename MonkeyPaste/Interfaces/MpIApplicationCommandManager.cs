using System.Windows.Input;

namespace MonkeyPaste {
    public interface MpIApplicationCommandManager {
        ICommand PerformApplicationCommand { get; }
    }
}
