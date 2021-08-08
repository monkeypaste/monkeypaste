using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Reflection;
using Xamarin.Forms;

namespace MonkeyPaste {
    public class MpViewModelBase : MpObservableObject {
        #region Private Variables
        #endregion        

        #region Properties

        #region View Models
        //public MpMainShellViewModel MainShellViewModel {
        //    get {
        //        return Application.Current.MainPage.BindingContext as MpMainShellViewModel;
        //    }
        //}
        public MpViewModelBase ParentViewModel { get; set; }
        #endregion

        public MpINavigate Navigation { get; set; } = new MpNavigator();
        public static string User { get; set; }

        
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

        

        #region Protected Methods
        protected MpViewModelBase() : base() { }

        #endregion

        #region Private methods
        #endregion

        
    }
}
