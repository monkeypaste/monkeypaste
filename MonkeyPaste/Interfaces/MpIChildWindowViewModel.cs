namespace MonkeyPaste {
    public interface MpIChildWindowViewModel : MpIWindowViewModel {
        bool IsOpen { get; set; }
    }
    public interface MpIWindowHandlesClosingViewModel : MpIViewModel {
        bool IsCloseHandled { get; }
    }
}
