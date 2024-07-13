using System.Windows.Input;

namespace MonkeyPaste.Avalonia {
    public interface MpIContentQueryTextBoxViewModel : MpITextSelectionRange {
        bool CanPopOut { get; }
        bool IsWindowOpen { get; }
        bool IsReadOnly { get; }
        bool IsFieldButtonVisible { get; }
        bool IsActionParameter { get; }
        string ContentQuery { get; set; }
        string Watermark { get; }
        bool IsSecure { get; }
        ICommand ClearQueryCommand { get; }
        ICommand ShowQueryMenuCommand { get; }
        ICommand OpenPopOutWindowCommand { get; }
    }
}
