using System.Windows.Input;

namespace MonkeyPaste {
    public interface MpIContentQueryTextBoxViewModel {
        bool IsActionParameter { get; set; }
        string ContentQuery { get; set; }
        ICommand ShowContentPathSelectorMenuCommand { get; }
        ICommand ClearQueryCommand { get; }
    }
}
