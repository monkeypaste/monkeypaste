namespace MonkeyPaste.Avalonia {
    public interface MpISliderViewModel : MpIViewModel {
        double SliderValue { get; set; }
        double MinValue { get; }
        double MaxValue { get; }
        int Precision { get; }
    }
}
