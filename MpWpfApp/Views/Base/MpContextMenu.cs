using System.ComponentModel;
using System.Windows.Controls;

namespace MpWpfApp {
    public abstract class MpContextMenu : ContextMenu { }

    public class MpContextMenu<T> : MpContextMenu where T: class {
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

        public MpContextMenu() : base() {
        }
    }
}
