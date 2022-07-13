using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace MonkeyPaste.Common {
    public abstract class MpShape : INotifyPropertyChanged {
        public event PropertyChangedEventHandler PropertyChanged;
        public virtual MpPoint[] Points { get; set; }
        public MpShape() {
            Points = new MpPoint[] { };
        }
    }
}
