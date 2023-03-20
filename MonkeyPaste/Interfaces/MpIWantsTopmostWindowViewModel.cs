namespace MonkeyPaste {
    //public enum MpNotifierType {
    //    Default = 0,
    //    Startup,
    //    Dialog
    //}

    public interface MpIWantsTopmostWindowViewModel : MpIWindowViewModel {
        bool WantsTopmost { get; }
    }
}
