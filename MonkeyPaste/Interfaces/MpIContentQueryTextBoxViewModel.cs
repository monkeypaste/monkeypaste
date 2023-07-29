using System.Windows.Input;

namespace MonkeyPaste {
    public interface MpIContentQueryTextBoxViewModel : MpITextSelectionRange {
        bool IsFieldButtonVisible { get; }
        bool IsActionParameter { get; set; }
        string ContentQuery { get; set; }
        string Watermark { get; }
        ICommand ClearQueryCommand { get; }
        ICommand ShowQueryMenuCommand { get; }
    }
}
