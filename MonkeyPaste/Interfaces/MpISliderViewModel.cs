using System;
using System.Collections.Generic;
using System.Text;

namespace MonkeyPaste {
    public interface MpISliderViewModel : MpIViewModel {
        double SliderValue { get; }
        double MinValue { get; }
        double MaxValue { get; }
        int Precision { get; }
    }
}
