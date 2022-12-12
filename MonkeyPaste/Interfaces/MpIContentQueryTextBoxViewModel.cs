using System.Windows.Input;

namespace MonkeyPaste {
    public interface MpIContentQueryTextBoxViewModel : MpITextSelectionRange {
        bool IsPathSelectorPopupOpen { get; set; }
        bool IsActionParameter { get; set; }
        string ContentQuery { get; set; }
        ICommand ClearQueryCommand { get; }
    }
}
