namespace MonkeyPaste {

    public interface MpIChildWindowViewModel : MpIWindowViewModel {
        bool IsOpen { get; set; }
    }
    public interface MpIWindowHandlesClosingViewModel : MpIViewModel {
        bool IsCloseHandled { get; }
    }
    public interface MpIWindowStateViewModel : MpIViewModel {
        MpWindowState WindowState { get; set; }
    }
}
