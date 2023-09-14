using System.Windows.Input;

namespace MonkeyPaste.Avalonia {
    public interface MpAvIPopupSelectorMenuViewModel : MpIViewModel {
        bool IsOpen { get; set; }
        MpAvMenuItemViewModel PopupMenu { get; }
        ICommand ShowSelectorMenuCommand { get; }
        object SelectedIconResourceObj { get; }
        string SelectedLabel { get; }
    }
}
