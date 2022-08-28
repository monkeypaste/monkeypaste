using Avalonia.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PropertyChanged;

namespace MonkeyPaste.Avalonia {
    [DoNotNotify]
    public abstract class MpAvAdornerBase : Control {
        protected Control AdornedControl { get; private set; }

        public MpAvAdornerBase(Control adornedControl) : base() {            
            AdornedControl = adornedControl;
        }
    }
}
