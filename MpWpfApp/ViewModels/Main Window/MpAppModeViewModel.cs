using GalaSoft.MvvmLight.CommandWpf;
using Gma.System.MouseKeyHook;
using System;
using System.Windows;
using System.Windows.Input;

namespace MpWpfApp {
    public class MpAppModeViewModel : MpViewModelBase {
        #region View Models
        public MpMainWindowViewModel MainWindowViewModel { get; set; }

        #endregion

        #region Properties
        private bool _isInAppendMode = false;
        public bool IsInAppendMode {
            get {
                return _isInAppendMode;
            }
            set {
                if (_isInAppendMode != value) {
                    _isInAppendMode = value;
                    Console.WriteLine("IsInAppendMode changed to: " + _isInAppendMode);
                    OnPropertyChanged(nameof(IsInAppendMode));
                }
            }
        }

        private bool _isAutoCopyMode = false;
        public bool IsAutoCopyMode {
            get {
                return _isAutoCopyMode;
            }
            set {
                if (_isAutoCopyMode != value) {
                    _isAutoCopyMode = value;
                    OnPropertyChanged(nameof(IsAutoCopyMode));
                }
            }
        }

        private bool _isAppPaused = false;
        public bool IsAppPaused {
            get {
                return _isAppPaused;
            }
            set {
                if (_isAppPaused != value) {
                    _isAppPaused = value;
                    OnPropertyChanged(nameof(IsAppPaused));
                }
            }
        }
        #endregion

        private void GlobalMouseUpEvent(object sender, System.Windows.Forms.MouseEventArgs e) {
            if (e.Button == System.Windows.Forms.MouseButtons.Left && !MpHelpers.ApplicationIsActivated()) {
                System.Windows.Forms.SendKeys.SendWait("^c");
            }
        }

        #region Public Methods
        public MpAppModeViewModel(MpMainWindowViewModel parent) {
            MainWindowViewModel = parent;
            PropertyChanged += (s, e) => {
                switch (e.PropertyName) {
                    case nameof(IsAutoCopyMode):
                        if (IsAutoCopyMode) {
                            MainWindowViewModel.GlobalHook.MouseUp += GlobalMouseUpEvent;
                        } else {
                            MainWindowViewModel.GlobalHook.MouseUp -= GlobalMouseUpEvent;
                        }
                        break;
                }
            };
        }

        public void AppMode_Loaded(object sender, RoutedEventArgs e) {

        }
        #endregion

        #region Commands
        private RelayCommand _toggleAppendModeCommand;
        public ICommand ToggleAppendModeCommand {
            get {
                if (_toggleAppendModeCommand == null) {
                    _toggleAppendModeCommand = new RelayCommand(ToggleAppendMode, CanToggleAppendMode);
                }
                return _toggleAppendModeCommand;
            }
        }
        private bool CanToggleAppendMode() {
            //only allow append mode to activate if app is not paused and only ONE clip is selected
            return !IsAppPaused && MainWindowViewModel.ClipTrayViewModel.SelectedClipTiles.Count == 1;
        }
        private void ToggleAppendMode() {
            IsInAppendMode = !IsInAppendMode;
        }

        private RelayCommand _toggleAutoCopyModeCommand;
        public ICommand ToggleAutoCopyModeCommand {
            get {
                if (_toggleAutoCopyModeCommand == null) {
                    _toggleAutoCopyModeCommand = new RelayCommand(ToggleAutoCopyMode, CanToggleAutoCopyMode);
                }
                return _toggleAutoCopyModeCommand;
            }
        }
        private bool CanToggleAutoCopyMode() {
            //only allow append mode to activate if app is not paused and only ONE clip is selected
            return !IsAppPaused;
        }
        private void ToggleAutoCopyMode() {
            IsAutoCopyMode = !IsAutoCopyMode;
        }
        #endregion
    }
}
