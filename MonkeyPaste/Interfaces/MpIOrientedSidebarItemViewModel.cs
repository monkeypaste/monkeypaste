namespace MonkeyPaste {
    public interface MpISidebarItemViewModel : MpIViewModel, MpISelectableViewModel {
        double DefaultSidebarWidth { get; }
        double DefaultSidebarHeight { get; }
        double SidebarWidth { get; set; }
        double SidebarHeight { get; set; }

        string SidebarBgHexColor { get; }
        bool CanResize { get; }
    }
}
