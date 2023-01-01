namespace MonkeyPaste {
    public interface MpIPopupSelectorMenu {
        bool IsOpen { get; set; }
        MpMenuItemViewModel PopupMenu { get; }
        //MpMenuItemViewModel SelectedMenuItem { get; }
        object SelectedIconResourceObj { get; }
        string SelectedLabel { get; }

        //string EmptyText { get; }
        //object EmptyIconResourceObj { get; }
    }
}
