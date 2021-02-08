using GalaSoft.MvvmLight.CommandWpf;
using Gma.System.MouseKeyHook;
using System;
using System.Windows;
using System.Windows.Input;

namespace MpWpfApp {
    public class MpAppModeViewModel : MpViewModelBase {
        #region View Models
        #endregion

        #region Properties

        private bool _isRightClickPasteMode = false;
        public bool IsRightClickPasteMode {
            get {
                return _isRightClickPasteMode;
            }
            set {
                if (_isRightClickPasteMode != value) {
                    _isRightClickPasteMode = value;
                    Console.WriteLine("IsRightClickPasteMode changed to: " + _isRightClickPasteMode);
                    OnPropertyChanged(nameof(IsRightClickPasteMode));
                }
            }
        }

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

        #region Public Methods
        public MpAppModeViewModel() : base() {
            PropertyChanged += (s, e) => {
                switch (e.PropertyName) {
                    case nameof(IsInAppendMode):
                        if(Properties.Settings.Default.NotificationShowModeChangeToast) {
                            string status = IsInAppendMode ? "ON" : "OFF";
                            MainWindowViewModel.SystemTrayViewModel.ShowStandardBalloon("Monkey Paste", "Append Mode is " + status, Hardcodet.Wpf.TaskbarNotification.BalloonIcon.Info);
                        }
                        break;
                }
            };
        }

        public void AppMode_Loaded(object sender, RoutedEventArgs args) {
            MainWindowViewModel.GlobalHook.MouseUp += (s, e) => {
                if (IsAutoCopyMode) {
                    if (e.Button == System.Windows.Forms.MouseButtons.Left && !MpHelpers.Instance.ApplicationIsActivated()) {
                        System.Windows.Forms.SendKeys.SendWait("^c");
                    }
                }
                if (IsRightClickPasteMode) {
                    if (e.Button == System.Windows.Forms.MouseButtons.Right && !MpHelpers.Instance.ApplicationIsActivated()) {
                        System.Windows.Forms.SendKeys.SendWait("^v");
                    }
                }
            };
        }
        #endregion

        #region Commands
        private RelayCommand _toggleRightClickPasteCommand;
        public ICommand ToggleRightClickPasteCommand {
            get {
                if (_toggleRightClickPasteCommand == null) {
                    _toggleRightClickPasteCommand = new RelayCommand(ToggleRightClickPaste, CanToggleRightClickPaste);
                }
                return _toggleRightClickPasteCommand;
            }
        }
        private bool CanToggleRightClickPaste() {
            //only allow append mode to activate if app is not paused and only ONE clip is selected
            return !IsAppPaused;
        }
        private void ToggleRightClickPaste() {
            IsRightClickPasteMode = !IsRightClickPasteMode;
        }

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
            return !IsAppPaused;
        }
        private void ToggleAutoCopyMode() {
            IsAutoCopyMode = !IsAutoCopyMode;
        }
        #endregion
    }
}
