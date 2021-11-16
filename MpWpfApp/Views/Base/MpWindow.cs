using System.Windows;
using WPFSpark;

namespace MpWpfApp {

    public class MpWindow : SparkWindow {
        public MpWindow() : base() { }
    }

    public class MpWindow<T> : MpWindow where T: class {
        public T BindingContext {
            get {
                if (System.ComponentModel.DesignerProperties.GetIsInDesignMode(this)) {
                    return null;
                }

                if (DataContext == null) {
                    return null;
                }
                return (T)GetValue(DataContextProperty);
            }
            set {
                if (System.ComponentModel.DesignerProperties.GetIsInDesignMode(this)) {
                    return;
                }
                if (BindingContext != value) {
                    SetValue(DataContextProperty, value);
                }
            }
        }
        public static readonly DependencyProperty BindingContextProperty =
            DependencyProperty.Register(
                "BindingContext",
                typeof(T),
                typeof(MpUserControl<T>),
                new FrameworkPropertyMetadata(null));
    }
}
