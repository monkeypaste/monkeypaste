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
        public MpShape() {
            //Points = new MpPoint[] { };
        }
    }
}
