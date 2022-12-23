namespace MonkeyPaste {
    public interface MpIPopupSelectorMenu {
        bool IsOpen { get; set; }
        MpMenuItemViewModel PopupMenu { get; }
        MpMenuItemViewModel SelectedMenuItem { get; }
        string EmptyText { get; }
        object EmptyIconResourceObj { get; }
    }
}
