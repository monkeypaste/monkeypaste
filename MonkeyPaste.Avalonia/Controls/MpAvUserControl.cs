using Avalonia.Controls;
using PropertyChanged;

namespace MonkeyPaste.Avalonia {
    [DoNotNotify]
    public abstract class MpAvUserControl : UserControl, MpIUserControl {
        public MpAvUserControl() : base() { }

        public void SetDataContext(object dataContext) {
            DataContext = dataContext;
        }
    }
    public abstract class MpAvUserControl<T> : MpAvUserControl where T : class {
        public T BindingContext {
            get => GetValue(DataContextProperty) as T;
            set => SetValue(DataContextProperty, value);
        }

        public MpAvUserControl() : base() {
        }

    }

}
