using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;

namespace MpWpfApp {
    public class MpViewModelBase : INotifyPropertyChanged {
        //private static List<MpViewModelBase> _ViewModelList = new List<MpViewModelBase>();
        public MpMainWindowViewModel MainWindowViewModel {
            get {
                return (MpMainWindowViewModel)((MpMainWindow)Application.Current.MainWindow).DataContext;
            }
        }

        public bool ThrowOnInvalidPropertyName { get; private set; }

        //private bool _isFocused = false;
        //public bool IsFocused {
        //    get {
        //        return _isFocused;
        //    }
        //    set {
        //        //omitting duplicate check to enforce change in ui
        //        //if (_isFocused != value) 
        //        {
        //            _isFocused = value;
        //            OnPropertyChanged(nameof(IsFocused));
        //        }
        //    }
        //}

        public event PropertyChangedEventHandler PropertyChanged;

        public virtual void OnPropertyChanged(string propertyName) {
            this.VerifyPropertyName(propertyName);
            PropertyChangedEventHandler handler = this.PropertyChanged;
            if (handler != null) {
                var e = new PropertyChangedEventArgs(propertyName);
                handler(this, e);
            }
        }

        [Conditional("DEBUG")]
        [DebuggerStepThrough]
        public void VerifyPropertyName(string propertyName) {
            // Verify that the property name matches a real, 
            // public, instance property on this object. 
            if (TypeDescriptor.GetProperties(this)[propertyName] == null) {
                string msg = "Invalid property name: " + propertyName;
                if (this.ThrowOnInvalidPropertyName) {
                    throw new Exception(msg);
                } else {
                    Debug.Fail(msg);
                }
            }
        }

        //public MpViewModelBase() {
            //PropertyChanged += (s, e) => {
            //    switch(e.PropertyName) {
            //        case nameof(IsFocused):
            //            var focusedVm = (MpViewModelBase)s;
            //            if(focusedVm.IsFocused) {
            //                foreach (var vm in _ViewModelList) {
            //                    if (vm == focusedVm) {
            //                        continue;
            //                    }
            //                    vm.IsFocused = false;
            //                }
            //            }
            //            break;
            //    }
            //};
            //_ViewModelList.Add(this);
        //}

        public virtual bool InitHotkeys() {
            //do nothing this is overriden in actual viewmodels
            return true;
        }
    }
}
