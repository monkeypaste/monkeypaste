using System;
using System.Collections.Generic;
using System.Text;

namespace MonkeyPaste {
    public interface MpISliderViewModel {
        double TotalWidth { get; }
        double SliderValue { get; set; }
        double MinValue { get; }
        double MaxValue { get; }
        int Precision { get; }
    }
}
