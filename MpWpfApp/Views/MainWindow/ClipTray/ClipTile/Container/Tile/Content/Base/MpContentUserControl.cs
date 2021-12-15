using Microsoft.Xaml.Behaviors;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace MpWpfApp {
    public abstract class MpContentUserControl<T> : MpUserControl<T> 
        where T: class {
        
        public new MpContentItemViewModel BindingContext {
            get {
                if (System.ComponentModel.DesignerProperties.GetIsInDesignMode(this) ||
                    GetValue(DataContextProperty) == null//.GetType() != typeof(T)
                        ) {
                    return null;
                }
                return (MpContentItemViewModel)GetValue(DataContextProperty);
            }
            set {
                if (System.ComponentModel.DesignerProperties.GetIsInDesignMode(this)) {
                    return;
                }

                SetValue(DataContextProperty, value);
            }
        }

        
    }
}
