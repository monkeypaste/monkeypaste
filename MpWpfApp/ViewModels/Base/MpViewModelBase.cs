using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
using System.Windows.Markup;
using System.Globalization;
using System.Windows.Controls;
using System.Reflection;
using MonkeyPaste;

namespace MpWpfApp {    
    public abstract class MpViewModelBase<P> : DependencyObject, INotifyPropertyChanged where P: class {
        #region Private Variables

        #endregion

        #region Properties

        private P _parent;
        public P Parent {
            get {
                return _parent;
            }
            set {
                if(_parent != value) {
                    _parent = value;
                    OnPropertyChanged_old(nameof(Parent));
                }
            }
        }


        #region View Models
        public MpMainWindowViewModel MainWindowViewModel {
            get {
                return (MpMainWindowViewModel)((MpMainWindow)Application.Current.MainWindow).DataContext;
                //object mwvm = null;
                //Application.Current.Dispatcher.Invoke((Action)delegate {
                //    mwvm = (MpMainWindowViewModel)((MpMainWindow)Application.Current.MainWindow).DataContext;
                //});
                //return mwvm as MpMainWindowViewModel;
            }
        }

        //public MpMeasurements Measurements { get; private set; }
        #endregion

        public bool HasViewChanged { get; set; }

        private bool _isMouseOverVerticalScrollBar = false;
        public bool IsMouseOverVerticalScrollBar {
            get {
                return _isMouseOverVerticalScrollBar;
            }
            set {
                if (_isMouseOverVerticalScrollBar != value) {
                    _isMouseOverVerticalScrollBar = value;
                    OnPropertyChanged_old(nameof(IsMouseOverVerticalScrollBar));
                    OnPropertyChanged_old(nameof(IsMouseOverScrollBar));
                }
            }
        }

        private bool _isMouseOverHorizontalScrollBar = false;
        public bool IsMouseOverHorizontalScrollBar {
            get {
                return _isMouseOverHorizontalScrollBar;
            }
            set {
                if (_isMouseOverHorizontalScrollBar != value) {
                    _isMouseOverHorizontalScrollBar = value;
                    OnPropertyChanged_old(nameof(IsMouseOverHorizontalScrollBar));
                    OnPropertyChanged_old(nameof(IsMouseOverScrollBar));
                }
            }
        }

        public bool IsMouseOverScrollBar {
            get {
                return IsMouseOverHorizontalScrollBar || IsMouseOverVerticalScrollBar;
            }
        }
        public bool CanAcceptChildren { get; set; } = true;

        public static bool IsTrialExpired {
            get {
                return MonkeyPaste.MpPreferences.Instance.IsTrialExpired;
            }
            set {
                if (MonkeyPaste.MpPreferences.Instance.IsTrialExpired != value) {
                    MonkeyPaste.MpPreferences.Instance.IsTrialExpired = value;
                }
            }
        }

        private bool _isBusy = false;
        public bool IsBusy {
            get {
                return _isBusy;
            }
            set {
                if(_isBusy != value) {
                    _isBusy = value;
                    OnPropertyChanged_old(nameof(IsBusy));
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
                    OnPropertyChanged_old(nameof(Name));
                }
            }
        }

        private Visibility _itemVisibility = Visibility.Visible;
        public Visibility ItemVisibility {
            get {
                return _itemVisibility;
            }
            set {
                if(_itemVisibility != value) {
                    _itemVisibility = value;
                    OnPropertyChanged_old(nameof(ItemVisibility));
                }
            }
        }
        #endregion

        #region Events
        public event EventHandler ViewModelLoaded;
        protected virtual void OnViewModelLoaded() => ViewModelLoaded?.Invoke(this, EventArgs.Empty);
        #endregion

        #region Public Methods
        #endregion

        #region Protected Methods
        protected MpViewModelBase(P parent) {
            Parent = parent;
            MpDb.Instance.OnItemAdded += Instance_OnItemAdded;
            MpDb.Instance.OnItemUpdated += Instance_OnItemUpdated;
            MpDb.Instance.OnItemDeleted += Instance_OnItemDeleted;
            MpDb.Instance.SyncAdd += Instance_SyncAdd;
            MpDb.Instance.SyncUpdate += Instance_SyncUpdate;
            MpDb.Instance.SyncDelete += Instance_SyncDelete;
        }

        #region Db Events

        protected virtual void Instance_SyncDelete(object sender, MpDbSyncEventArgs e) {
            
        }

        protected virtual void Instance_SyncUpdate(object sender, MpDbSyncEventArgs e) {  }

        protected virtual void Instance_SyncAdd(object sender, MpDbSyncEventArgs e) { }

        protected virtual void Instance_OnItemDeleted(object sender, MpDbModelBase e) { }

        protected virtual void Instance_OnItemUpdated(object sender, MpDbModelBase e) { }

        protected virtual void Instance_OnItemAdded(object sender, MpDbModelBase e) { }

        #endregion

        #endregion

        #region Private methods

        #endregion

        //#region INotifyPropertyChanged 
        //public bool ThrowOnInvalidPropertyName { get; private set; } = false;

        //private event PropertyChangedEventHandler _propertyChanged;
        //public event PropertyChangedEventHandler PropertyChanged {
        //    add { _propertyChanged += value; }
        //    remove { _propertyChanged -= value; }
        //}

        public virtual void OnPropertyChanged_old(string propertyName) {
            //PropertyChangedEventHandler handler = PropertyChanged;
            //if (handler != null) {
            //    var e = new PropertyChangedEventArgs(propertyName);
            //    handler(this, e);
            //}
        }

        public virtual void OnPropertyChanged(string propertyName) {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) {
                var e = new PropertyChangedEventArgs(propertyName);
                handler(this, e);
            }
        }
        //#endregion

        #region PropertyChanged 
        public event PropertyChangedEventHandler PropertyChanged;

        public bool IsChanged { get; set; }
        #endregion
    }
}
