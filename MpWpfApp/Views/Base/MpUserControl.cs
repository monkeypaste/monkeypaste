using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace MpWpfApp {
    public class MpUserControl : UserControl {
        public MpUserControl() : base() { }

        public static readonly DependencyProperty ResourcesProperty =
            DependencyProperty.Register("Resources", typeof(ResourceDictionary), typeof(MpUserControl), new PropertyMetadata(null));

    }
    public class MpUserControl<T> : MpUserControl where T: class {

        public T BindingContext {
            get {
                if (DataContext == null) {
                    return null;
                }
                return (T)GetValue(DataContextProperty);
            }
            set {
                if (BindingContext != value) {
                    SetValue(DataContextProperty, value);
                }
            }
        }
        public static readonly DependencyProperty BindingContextProperty =
            DependencyProperty.Register("BindingContext", typeof(T), typeof(MpUserControl<T>), new PropertyMetadata(null));

        
        public MpUserControl() : base() { }

    }
}
