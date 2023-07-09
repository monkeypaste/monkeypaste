namespace MonkeyPaste {
    public interface MpIBoundSizeViewModel : MpIViewModel {
        double ContainerBoundWidth { get; set; }
        double ContainerBoundHeight { get; set; }
    }

    public interface MpIAnimatedSizeViewModel : MpIBoundSizeViewModel {
        bool IsAnimating { get; set; }
    }
}
