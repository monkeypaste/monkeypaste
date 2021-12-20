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

namespace MpWpfApp {
    public abstract class MpUserControl : UserControl {
        //private ObservableCollection<Behavior> _behaviors = new ObservableCollection<Behavior>();

        public MpUserControl() : base() {
            OnLoad();
        }

        public CancellationTokenSource CTS;

        public void RegisterBehavior(Behavior b) {
            //if(b == null) {
            //    return;
            //}
            //if (_behaviors.Contains(b)) {
            //    _behaviors[_behaviors.IndexOf(b)] = b;
            //    return;
            //}
            //_behaviors.Add(b);
        }

        public void UnregisterBehavior(Behavior b) {
            //if (b == null) {
            //    return;
            //}
            //if (_behaviors.Contains(b)) {
            //    _behaviors.Remove(b);
            //}
        }

        protected virtual void RegisterAllBehaviors() { }

        protected virtual void OnLoad() {
            CTS = new CancellationTokenSource();
            //RegisterAllBehaviors();
        }

        protected virtual void OnUnload() {
            //if (CTS != null) {
            //    CTS.Cancel();
            //    CTS.Dispose();
            //}
            //_behaviors.ForEach(x => x.Detach());
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
