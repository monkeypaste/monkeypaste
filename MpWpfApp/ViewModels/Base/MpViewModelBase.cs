using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
using System.Windows.Markup;
using System.Globalization;
using System.Windows.Controls;

namespace MpWpfApp {
    public class MpViewModelBase : DependencyObject, INotifyPropertyChanged {
        #region Private Variables

        #endregion

        #region View Models
        public MpMainWindowViewModel MainWindowViewModel {
            get {
                return (MpMainWindowViewModel)((MpMainWindow)Application.Current.MainWindow).DataContext;
            }
        }
        #endregion

        #region Properties
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
                if(_isBusy != value) {
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
        #endregion

        #region Protected Methods
        protected MpViewModelBase() {
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

            SetOSCultureBinding();
        }

        /// <summary>
        /// With this method, we can inject design time data into the view so that we can
        /// create a more Blendable application.
        /// </summary>
        protected virtual void DesignData() {  }


        #endregion

        #region Private methods
        /// <summary>
        /// Set the current culture binding based on the OS culture.
        /// </summary>
        private static void SetOSCultureBinding() {
            if (!_osBinding && !_designMode) {
                FrameworkElement.LanguageProperty.OverrideMetadata(
                     typeof(FrameworkElement), new FrameworkPropertyMetadata(
                         XmlLanguage.GetLanguage(CultureInfo.CurrentCulture.IetfLanguageTag)));
                _osBinding = true;
            }
        }
        #endregion

        #region IPropertyChanged 
        public bool ThrowOnInvalidPropertyName { get; private set; }

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
