﻿using Prism.Commands;
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
        private DelegateCommand _loadedCommand;
        public ICommand LoadedCommand {
            get {
                if(_loadedCommand == null) {
                    _loadedCommand = new DelegateCommand(Loaded, IsLoaded);
                }
                return _loadedCommand;
            }
        }
        protected virtual void Loaded() { }            
        
        private bool IsLoaded() {
            return App.Current.MainWindow != null && App.Current.MainWindow.IsLoaded;
        }

        private DelegateCommand exitCommand;
        public ICommand ExitCommand {
            get {
                if(exitCommand == null) {
                    exitCommand = new DelegateCommand(Exit);
                }
                return exitCommand;
            }
        }
        private void Exit() {
            Application.Current.Shutdown();
        }

        private DelegateCommand closedCommand;
        public ICommand ClosedCommand {
            get {
                if(closedCommand == null) {
                    closedCommand = new DelegateCommand(Closed);
                }
                return closedCommand;
            }
        }
        private void Closed() {
            //log.Add("You won't see this of course! Closed command executed");
            //MessageBox.Show("Closed");
        }

        private DelegateCommand closingCommand;
        public ICommand ClosingCommand {
            get {
                if(closingCommand == null) {
                    closingCommand = new DelegateCommand(
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

        private DelegateCommand cancelClosingCommand;
        public ICommand CancelClosingCommand {
            get {
                if(cancelClosingCommand == null) {
                    cancelClosingCommand = new DelegateCommand(CancelClosing);
                }
                return cancelClosingCommand;
            }
        }
        private void CancelClosing() {
            //log.Add("CancelClosing command executed");
            MessageBox.Show("CancelClosing");
        }

        private MpRelayCommand _showWindowCommand;
        public ICommand ShowWindowCommand {
            get {
                if(_showWindowCommand == null) {
                    _showWindowCommand = new MpRelayCommand(
                        param => this.ShowWindow(),
                        param => this.CanShowWindow());
                }
                return _showWindowCommand;
            }
        }
        private bool CanShowWindow() {
            return Application.Current.MainWindow == null || Application.Current.MainWindow.Visibility != Visibility.Visible;
        }
        private int ShowWindow() {
            if(Application.Current.MainWindow == null) {
                Application.Current.MainWindow = new MpMainWindow();
            }
            Application.Current.MainWindow.Show();
            Application.Current.MainWindow.Activate();
            Application.Current.MainWindow.Visibility = Visibility.Visible;
            return 3;
        }

        private MpRelayCommand _hideWindowCommand;
        public ICommand HideWindowCommand {
            get {
                if(_hideWindowCommand == null) {
                    _hideWindowCommand = new MpRelayCommand(
                        param => this.HideWindow(),
                        param => this.CanHideWindow());
                }
                return _hideWindowCommand;
            }
        }
        private void HideWindow() {
            Application.Current.MainWindow.Visibility = Visibility.Hidden;
        }
        private bool CanHideWindow() {
            return Application.Current.MainWindow != null && Application.Current.MainWindow.IsVisible == true;
        }


        public ICommand ExitApplicationCommand {
            get {
                return new MpRelayCommand(param => {
                    Application.Current.MainWindow.Close();
                    Application.Current.Shutdown();
                });
            }
        }
        #endregion
    }
}