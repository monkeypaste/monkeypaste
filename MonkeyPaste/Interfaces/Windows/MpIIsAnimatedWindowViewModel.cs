namespace MonkeyPaste {
    public interface MpIIsAnimatedWindowViewModel : MpIViewModel {
        bool IsAnimated { get; }
        bool IsAnimating { get; set; }
        bool IsComplete { get; }
    }
}
