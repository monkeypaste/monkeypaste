using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Text;

namespace MonkeyPaste {
    public class MpObservableObject : INotifyPropertyChanged {
        #region Events
        public event EventHandler ViewModelLoaded;
        protected virtual void OnViewModelLoaded() => ViewModelLoaded?.Invoke(this, EventArgs.Empty);
        #endregion

        #region Protected Methods
        protected MpObservableObject() : base() { }

        public void RaisePropertyChanged(params string[] propertyNames) {
            foreach (var propertyName in propertyNames) {
                OnPropertyChanged(propertyName);
                //_propertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        #endregion


        #region INotifyPropertyChanged 
        public bool ThrowOnInvalidPropertyName { get; private set; } = true;

        private event PropertyChangedEventHandler _propertyChanged;
        public event PropertyChangedEventHandler PropertyChanged {
            add { _propertyChanged += value; }
            remove { _propertyChanged -= value; }
        }

        public virtual void OnPropertyChanged(string propertyName) {
            this.VerifyPropertyName(propertyName);
            PropertyChangedEventHandler handler = _propertyChanged;
            if (handler != null) {
                var e = new PropertyChangedEventArgs(propertyName);
                handler(this, e);
            }
        }

        //[Conditional("DEBUG")]
        //[DebuggerStepThrough]
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
        #endregion
    }
}
