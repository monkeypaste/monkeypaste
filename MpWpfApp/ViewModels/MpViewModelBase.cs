using GalaSoft.MvvmLight.CommandWpf;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;


namespace MpWpfApp {
    public class MpViewModelBase : INotifyPropertyChanged  {
        public string DisplayName { get; set; }
        public bool ThrowOnInvalidPropertyName { get; private set; }

        private bool _isFocused;
        public  bool IsFocused {
            get {
                return _isFocused;
            }
            set {
                if(_isFocused != value) {
                    _isFocused = value;
                    OnPropertyChanged(nameof(IsFocused));
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName) {
            this.VerifyPropertyName(propertyName);
            PropertyChangedEventHandler handler = this.PropertyChanged;
            if(handler != null) {
                var e = new PropertyChangedEventArgs(propertyName);
                handler(this, e);
            }
        }
        [Conditional("DEBUG")]
        [DebuggerStepThrough]
        public void VerifyPropertyName(string propertyName) {
            // Verify that the property name matches a real, 
            // public, instance property on this object. 
            if(TypeDescriptor.GetProperties(this)[propertyName] == null) {
                string msg = "Invalid property name: " + propertyName;
                if(this.ThrowOnInvalidPropertyName)
                    throw new Exception(msg);
                else
                    Debug.Fail(msg);
            }
        }
        public bool IsInDesignMode {
            get {
                return DesignerProperties.GetIsInDesignMode(new DependencyObject());
            }
        }
        #region Commands
        private RelayCommand _loadedCommand;
        public ICommand LoadedCommand {
            get {
                if(_loadedCommand == null) {
                    _loadedCommand = new RelayCommand(Loaded, IsLoaded);
                }
                return _loadedCommand;
            }
        }
        protected virtual void Loaded() { }            
        
        private bool IsLoaded() {
            return App.Current.MainWindow != null && App.Current.MainWindow.IsLoaded;
        }

        private RelayCommand exitCommand;
        public ICommand ExitCommand {
            get {
                if(exitCommand == null) {
                    exitCommand = new RelayCommand(Exit);
                }
                return exitCommand;
            }
        }
        private void Exit() {
            Application.Current.Shutdown();
        }

        private RelayCommand closedCommand;
        public ICommand ClosedCommand {
            get {
                if(closedCommand == null) {
                    closedCommand = new RelayCommand(Closed);
                }
                return closedCommand;
            }
        }
        private void Closed() {
            //log.Add("You won't see this of course! Closed command executed");
            //MessageBox.Show("Closed");
        }

        private RelayCommand closingCommand;
        public ICommand ClosingCommand {
            get {
                if(closingCommand == null) {
                    closingCommand = new RelayCommand(
                        ExecuteClosing, CanExecuteClosing);
                }
                return closingCommand;
            }
        }
        private void ExecuteClosing() {
            //log.Add("Closing command executed");
            MessageBox.Show("Closing");
        }
        private bool CanExecuteClosing() {
            //log.Add("Closing command execution check");

            return MessageBox.Show("OK to close?", "Confirm",
                MessageBoxButton.YesNo) == MessageBoxResult.Yes;
        }

        private RelayCommand cancelClosingCommand;
        public ICommand CancelClosingCommand {
            get {
                if(cancelClosingCommand == null) {
                    cancelClosingCommand = new RelayCommand(CancelClosing);
                }
                return cancelClosingCommand;
            }
        }
        private void CancelClosing() {
            //log.Add("CancelClosing command executed");
            MessageBox.Show("CancelClosing");
        }

        public ICommand ExitApplicationCommand {
            get {
                return new RelayCommand(Application.Current.Shutdown);
            }
        }
        #endregion
    }
}
