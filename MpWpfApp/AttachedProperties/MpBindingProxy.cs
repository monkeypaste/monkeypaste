using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace MpWpfApp {
    public class MpBindingProxy : Freezable {
        #region Overrides of Freezable

        protected override Freezable CreateInstanceCore() {
            return new MpBindingProxy();
        }

        #endregion

        public object Data {
            get { 
                return (object)GetValue(DataProperty); 
            }
            set { 
                SetValue(DataProperty, value); 
            }
        }

        // Using a DependencyProperty as the backing store for Data.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty DataProperty =
            DependencyProperty.Register(
                "Data", 
                typeof(object), 
                typeof(MpBindingProxy), 
                new UIPropertyMetadata(null));
    }

    //public class MpBindingProxy<T> : Freezable where T: class{
    //    #region Overrides of Freezable

    //    protected override Freezable CreateInstanceCore() {
    //        return new MpBindingProxy();
    //    }

    //    #endregion

    //    public T Data {
    //        get {
    //            return (T)GetValue(DataProperty);
    //        }
    //        set {
    //            SetValue(DataProperty, value);
    //        }
    //    }

    //    // Using a DependencyProperty as the backing store for Data.  This enables animation, styling, binding, etc...
    //    public static readonly DependencyProperty DataProperty =
    //        DependencyProperty.Register(
    //            "Data",
    //            typeof(T),
    //            typeof(MpBindingProxy<T>),
    //            new UIPropertyMetadata(null));
    //}
}
