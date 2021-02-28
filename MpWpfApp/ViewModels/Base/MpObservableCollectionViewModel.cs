using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Data;

namespace MpWpfApp {
    public class MpObservableCollection<T> : ObservableCollection<T> {
        private object _ItemsLock = new object();

        public MpObservableCollection() : base() {
            BindingOperations.EnableCollectionSynchronization(this, _ItemsLock);
        }
        public MpObservableCollection(List<T> list) : base(list) {
            BindingOperations.EnableCollectionSynchronization(this, _ItemsLock);
        }
        public MpObservableCollection(IEnumerable<T> collection) : base(collection) {
            BindingOperations.EnableCollectionSynchronization(this, _ItemsLock);
        }
        public void OnCollectionChanged() {
            base.OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }
    }
    public class MpObservableCollectionViewModel<T> : MpObservableCollection<T> {
        public MpMainWindowViewModel MainWindowViewModel {
            get {
                return (MpMainWindowViewModel)((MpMainWindow)Application.Current.MainWindow).DataContext;
            }
        }

        #region Properties
        public bool CanAcceptChildren { get; set; } = true;
        #endregion

        public bool ThrowOnInvalidPropertyName { get; private set; }

        public new event PropertyChangedEventHandler PropertyChanged;

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
    }


}
