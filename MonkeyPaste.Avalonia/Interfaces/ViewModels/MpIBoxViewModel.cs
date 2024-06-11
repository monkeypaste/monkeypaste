namespace MonkeyPaste.Avalonia {

    public interface MpIBoxViewModel : MpIViewModel {
        double X { get; set; }
        double Y { get; set; }
        double Width { get; }
        double Height { get; }
    }
}
