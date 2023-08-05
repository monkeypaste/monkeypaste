namespace MonkeyPaste.Avalonia {
    public interface MpAvIPopupSelectorMenuViewModel : MpIViewModel {
        bool IsOpen { get; set; }
        MpAvMenuItemViewModel PopupMenu { get; }
        object SelectedIconResourceObj { get; }
        string SelectedLabel { get; }
    }
}
