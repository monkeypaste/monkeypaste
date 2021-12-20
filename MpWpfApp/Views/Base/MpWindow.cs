using System.ComponentModel;
using System.Windows;
using WPFSpark;

namespace MpWpfApp {
    public class MpWindow : Window {        
        protected object BindingObj {
            get {
                if (DesignerProperties.GetIsInDesignMode(this)) {
                    return null;
                }

                if (DataContext == null) {
                    return null;
                }
                return GetValue(DataContextProperty);
            }
            set {
                if (DesignerProperties.GetIsInDesignMode(this)) {
                    return;
                }
                if (BindingObj != value) {
                    SetValue(DataContextProperty, value);

                }
            }
        }
    }

    public class MpWindow<T> : MpWindow where T: class {
        public T BindingContext {
            get {
                if (DesignerProperties.GetIsInDesignMode(this)) {
                    return null;
                }

                if (DataContext == null) {
                    return null;
                }
                return (T)GetValue(DataContextProperty);
            }
            set {
                if (DesignerProperties.GetIsInDesignMode(this)) {
                    return;
                }
                if (BindingContext != value) {
                    SetValue(DataContextProperty, value);
                    BindingObj = BindingContext;
                }
            }
        }
    }
}
