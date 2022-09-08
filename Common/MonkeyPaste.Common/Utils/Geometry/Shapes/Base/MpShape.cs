using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace MonkeyPaste.Common {
    public abstract class MpShape : INotifyPropertyChanged {
        public event PropertyChangedEventHandler PropertyChanged;
        public abstract MpPoint[] Points { get; }

        public string FillOctColor { get; set; } = MpSystemColors.green1;
        public string StrokeOctColor { get; set; } = MpSystemColors.green4;
        public double StrokeThickness { get; set; } = 1.0d;
        public double[] StrokeDashStyle { get; set; } = new double[] { 1, 0 };
        public double StrokeDashOffset { get; set; } = 0;
        public string StrokeLineCap { get; set; } = "Flat";
        public string StrokeLineJoin { get; set; } = "Miter";
        public double StrokeMiterLimit { get; set; } = 10.0d;
        public MpShape() {
            //Points = new MpPoint[] { };
        }
    }
}
