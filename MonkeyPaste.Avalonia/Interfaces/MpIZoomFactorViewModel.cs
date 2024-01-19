using System.Windows.Input;

namespace MonkeyPaste.Avalonia {
    public interface MpIZoomFactorViewModel : MpIViewModel {
        double ZoomFactor { get; }
        double MinZoomFactor { get; }
        double MaxZoomFactor { get; }
        double DefaultZoomFactor { get; }
        double StepDelta { get; }

        ICommand ZoomInCommand { get; }
        ICommand ZoomOutCommand { get; }
        ICommand ResetZoomCommand { get; }
        ICommand SetZoomCommand { get; }

    }
}
