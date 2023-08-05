using Avalonia.Controls;
using Avalonia.ReactiveUI;
using PropertyChanged;
using ReactiveUI;

namespace MonkeyPaste.Avalonia {
    [DoNotNotify]
    public abstract class MpAvUserControl<TViewModel> : MpAvUserControl, MpIUserControl where TViewModel : class {
        public TViewModel BindingContext {
            get => GetValue(DataContextProperty) as TViewModel;
            set => SetValue(DataContextProperty, value);
        }

        public MpAvUserControl() : base() {
        }

    }

    [DoNotNotify]
    public abstract class MpAvUserControl : UserControl, MpIUserControl {
        public MpAvUserControl() : base() { }

        public void SetDataContext(object dataContext) {
            DataContext = dataContext;
        }
    }
    //[DoNotNotify]
    //public abstract class MpAvUserControl<TViewModel> : MpAvUserControl, IViewFor<TViewModel> where TViewModel : class {
    //    public TViewModel BindingContext {
    //        get => GetValue(DataContextProperty) as TViewModel;
    //        set => SetValue(DataContextProperty, value);
    //    }

    //    public TViewModel ViewModel {
    //        get => BindingContext;
    //        set => BindingContext = value;
    //    }

    //    object? IViewFor.ViewModel {
    //        get => ViewModel;
    //        set => ViewModel = (TViewModel)value;
    //    }

    //    public MpAvUserControl() : base() { }
    //}

}
