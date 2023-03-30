namespace MonkeyPaste {
    public interface MpIPopupSelectorMenuViewModel : MpIViewModel {
        bool IsOpen { get; set; }
        MpMenuItemViewModel PopupMenu { get; }
        //MpMenuItemViewModel SelectedMenuItem { get; }
        object SelectedIconResourceObj { get; }
        string SelectedLabel { get; }

        //string EmptyText { get; }
        //object EmptyIconResourceObj { get; }
    }
}
