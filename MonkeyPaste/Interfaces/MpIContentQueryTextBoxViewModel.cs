using System.Windows.Input;

namespace MonkeyPaste {
    public interface MpIContentQueryTextBoxViewModel : MpITextSelectionRange {
        bool IsReadOnly { get; }
        bool IsFieldButtonVisible { get; }
        bool IsActionParameter { get; set; }
        string ContentQuery { get; set; }
        string Watermark { get; }
        bool IsSecure { get; }
        ICommand ClearQueryCommand { get; }
        ICommand ShowQueryMenuCommand { get; }
    }
}
