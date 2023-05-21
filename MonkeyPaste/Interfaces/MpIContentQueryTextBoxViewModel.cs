using System.Windows.Input;

namespace MonkeyPaste {
    public interface MpIContentQueryTextBoxViewModel : MpITextSelectionRange {
        bool IsFieldButtonVisible { get; }
        bool IsPathSelectorPopupOpen { get; set; }
        bool IsActionParameter { get; set; }
        string ContentQuery { get; set; }
        ICommand ClearQueryCommand { get; }
    }
}
