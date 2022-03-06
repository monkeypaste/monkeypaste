using MpWpfApp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace MpWpfApp {
    public abstract class MpDragBehaviorBase<T> : MpBehavior<T> where T : FrameworkElement {
        public abstract bool CanDrag();

    }
}
