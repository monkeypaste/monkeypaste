using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

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
        public bool CanAcceptChildren { get; set; } = false;

        private bool _isTrialExpired = Properties.Settings.Default.IsTrialExpired;
        public bool IsTrialExpired {
            get {
                return _isTrialExpired;
            }
            set {
                if (_isTrialExpired != value) {
                    _isTrialExpired = value;
                    Properties.Settings.Default.IsTrialExpired = _isTrialExpired;
                    Properties.Settings.Default.Save();
                    OnPropertyChanged(nameof(IsTrialExpired));
                }
            }
        }

        private bool _isBusy;
        public bool IsBusy {
            get {
                return _isBusy;
            }
            protected set {
                if (_isBusy != value) {
                    _isBusy = value;
                    Application.Current.MainWindow.Cursor = IsBusy ? Cursors.Wait : Cursors.Arrow;
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

        //private static bool _osBinding = false;

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
        #endregion

        #region Protected Methods
        protected MpObservableCollectionViewModel() {
            OnInitialize();
        }

        protected virtual void OnInitialize() {
            _designMode = DesignerProperties.GetIsInDesignMode(new Button())
                || Application.Current == null || Application.Current.GetType() == typeof(Application);

            if (!_designMode) {
                var designMode = DesignerProperties.IsInDesignModeProperty;
                _designMode = (bool)DependencyPropertyDescriptor.FromProperty(designMode, typeof(FrameworkElement)).Metadata.DefaultValue;
            }

            if (_designMode) {
                DesignData();
            }
        }

        /// <summary>
        /// With this method, we can inject design time data into the view so that we can
        /// create a more Blendable application.
        /// </summary>
        protected virtual void DesignData() { }


        #endregion

        #region INotifyPropertyChanged Implementation

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
        #endregion
    }


}
