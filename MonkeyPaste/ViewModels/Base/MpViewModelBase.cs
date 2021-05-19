using System;
using System.ComponentModel;
using System.Diagnostics;
using Xamarin.Forms;

namespace MonkeyPaste {
    public class MpViewModelBase : INotifyPropertyChanged {
        #region Private Variables
        #endregion        

        #region Properties

        #region View Models
        public MpMainViewModel MainViewModel {
            get {
                return Application.Current.MainPage.BindingContext as MpMainViewModel;
            }
        }
        #endregion

        public MpINavigate Navigation { get; set; } = new MpNavigator();

        
        public bool CanAcceptChildren { get; set; } = true;

        private bool _isLoading;
        public bool IsLoading {
            get {
                return _isLoading;
            }
            set {
                if (_isLoading != value) {
                    _isLoading = value;
                    OnPropertyChanged(nameof(IsLoading));
                }
            }
        }


        private static bool _designMode = false;
        protected bool IsInDesignMode {
            get {
                return _designMode;
            }
        }

        private string _name = string.Empty;
        public string Name {
            get {
                return _name;
            }
            set {
                if (_name != value) {
                    _name = value;
                    OnPropertyChanged(nameof(Name));
                }
            }
        }

        private bool _isBusy;
        public bool IsBusy
        {
            get {
                return _isBusy;
            }
            set
            {
                if(_isBusy != value)
                {
                    _isBusy = value;
                    OnPropertyChanged(nameof(IsBusy));
                }
            }
        }
        public bool IsNotBusy => !IsBusy;

        #endregion

        #region Events
        public event EventHandler ViewModelLoaded;
        protected virtual void OnViewModelLoaded() => ViewModelLoaded?.Invoke(this, EventArgs.Empty);
        #endregion

        #region Protected Methods
        protected MpViewModelBase() { }

        public void RaisePropertyChanged(params string[] propertyNames) {
            foreach (var propertyName in propertyNames) {
                OnPropertyChanged(propertyName);
                //_propertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        #endregion

        #region Private methods
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
