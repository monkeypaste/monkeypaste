using Microsoft.Xaml.Behaviors;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using MonkeyPaste;

namespace MpWpfApp {

    public abstract class MpUserControl : UserControl, MpIUserControl {
        public MpUserControl() : base() { }

        public void SetDataContext(object dataContext) {
            DataContext = this;
        }
    }
    public class MpUserControl<T> : MpUserControl where T: class {
        public T BindingContext {
            get {
                if (DesignerProperties.GetIsInDesignMode(this) ||
                    GetValue(DataContextProperty) == null) {
                    return null;
                }
                return (T)GetValue(DataContextProperty);
            }
            set {
                if (DesignerProperties.GetIsInDesignMode(this)) {
                    return;
                }

                SetValue(DataContextProperty, value);
            }
        }

        public MpUserControl() : base() {
        }

    }
}
