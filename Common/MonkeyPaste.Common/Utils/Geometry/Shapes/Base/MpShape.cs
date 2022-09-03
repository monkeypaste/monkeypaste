using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace MonkeyPaste.Common {
    public abstract class MpShape : INotifyPropertyChanged {
        public event PropertyChangedEventHandler PropertyChanged;
        public abstract MpPoint[] Points { get; }
        public MpShape() {
            //Points = new MpPoint[] { };
        }
    }
}
