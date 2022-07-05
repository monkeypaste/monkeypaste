using Avalonia.Controls;
using MonkeyPaste;
using PropertyChanged;

namespace MonkeyPaste.Avalonia {
    [DoNotNotify]
    public abstract class MpAvUserControl : UserControl, MpIUserControl {
        public MpAvUserControl() : base() { }

        public void SetDataContext(object dataContext) {
            DataContext = dataContext;
        }
    }
    public abstract class MpAvUserControl<T> : MpAvUserControl where T: class {
        public T? BindingContext {
            get {
                if (Design.IsDesignMode ||
                    GetValue(DataContextProperty) == null) {
                    return null;
                }
                return GetValue(DataContextProperty) as T;
            }
            set {
                if (Design.IsDesignMode) {
                    return;
                }

                SetValue(DataContextProperty, value);
            }
        }

        public MpAvUserControl() : base() {
        }

    }

}
