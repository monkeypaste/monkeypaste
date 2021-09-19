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
                    OnPropertyChanged(nameof(Parent));
                }
            }
        }

        #region Property Reflection Referencer
        public object this[string propertyName] {
            get {
                // probably faster without reflection:
                // like:  return Properties.Settings.Default.PropertyValues[propertyName] 
                // instead of the following
                Type myType = typeof(MpClipTileViewModel);
                PropertyInfo myPropInfo = myType.GetProperty(propertyName);
                if (myPropInfo == null) {
                    throw new Exception("Unable to find property: " + propertyName);
                }
                return myPropInfo.GetValue(this, null);
            }
            set {
                Type myType = typeof(MpClipTileViewModel);
                PropertyInfo myPropInfo = myType.GetProperty(propertyName);
                myPropInfo.SetValue(this, value, null);
            }
        }
        #endregion

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


        private bool _isMouseOverVerticalScrollBar = false;
        public bool IsMouseOverVerticalScrollBar {
            get {
                return _isMouseOverVerticalScrollBar;
            }
            set {
                if (_isMouseOverVerticalScrollBar != value) {
                    _isMouseOverVerticalScrollBar = value;
                    OnPropertyChanged(nameof(IsMouseOverVerticalScrollBar));
                    OnPropertyChanged(nameof(IsMouseOverScrollBar));
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
                    OnPropertyChanged(nameof(IsMouseOverHorizontalScrollBar));
                    OnPropertyChanged(nameof(IsMouseOverScrollBar));
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
                    //Application.Current.MainWindow.Cursor = IsBusy ? Cursors.Wait : Cursors.Arrow;
                    OnPropertyChanged(nameof(IsBusy));
                }
            }            
        }

        private static bool _designMode = false;
        protected bool IsInDesignMode {
            get {
                return _designMode;
            }
        }

        private static bool _osBinding = false;

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

        private Visibility _itemVisibility = Visibility.Visible;
        public Visibility ItemVisibility {
            get {
                return _itemVisibility;
            }
            set {
                if(_itemVisibility != value) {
                    _itemVisibility = value;
                    OnPropertyChanged(nameof(ItemVisibility));
                }
            }
        }
        #endregion

        #region Events
        public event EventHandler ViewModelLoaded;
        protected virtual void OnViewModelLoaded() => ViewModelLoaded?.Invoke(this, EventArgs.Empty);
        #endregion

        #region Public Methods
        public void RaisePropertyChanged(params string[] propertyNames) {
            foreach (var propertyName in propertyNames) {
                OnPropertyChanged(propertyName);
                //_propertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }
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

        protected virtual void Instance_SyncDelete(object sender, MpDbSyncEventArgs e) {
            
        }

        protected virtual void Instance_SyncUpdate(object sender, MpDbSyncEventArgs e) {
            
        }

        protected virtual void Instance_SyncAdd(object sender, MpDbSyncEventArgs e) {
            
        }

        protected virtual void Instance_OnItemDeleted(object sender, MpDbModelBase e) {
            
        }

        protected virtual void Instance_OnItemUpdated(object sender, MpDbModelBase e) {
            
        }

        protected virtual void Instance_OnItemAdded(object sender, MpDbModelBase e) {
            
        }

        #endregion

        #region Private methods

        #endregion

        #region INotifyPropertyChanged 
        public bool ThrowOnInvalidPropertyName { get; private set; } = false;

        private event PropertyChangedEventHandler _propertyChanged;
        public event PropertyChangedEventHandler PropertyChanged {
            add { _propertyChanged += value; }
            remove { _propertyChanged -= value; }
        }

        public virtual void OnPropertyChanged(string propertyName) {
            PropertyChangedEventHandler handler = _propertyChanged;
            if (handler != null) {
                var e = new PropertyChangedEventArgs(propertyName);
                handler(this, e);
            }
        }
        #endregion
    }
}
