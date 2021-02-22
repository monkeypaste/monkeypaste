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

        #region Private Methods
        private void ShowNotifcation(bool fromHotkey, string modeType, string status, bool isOn) {
            //if(fromHotkey) 
            {
                if(Properties.Settings.Default.NotificationShowModeChangeToast) {
                    MpStandardBalloonViewModel.ShowBalloon("Monkey Paste", modeType + " is " + status);
                }
                if(Properties.Settings.Default.NotificationDoModeChangeSound) {
                    MpSoundPlayerGroupCollectionViewModel.Instance.PlayModeChangeCommand.Execute(isOn);
                }
                
            }
        }
        #endregion

        #region Commands
        private RelayCommand<bool> _toggleIsAppPausedCommand;
        public ICommand ToggleIsAppPausedCommand {
            get {
                if (_toggleIsAppPausedCommand == null) {
                    _toggleIsAppPausedCommand = new RelayCommand<bool>(ToggleIsAppPaused);
                }
                return _toggleIsAppPausedCommand;
            }
        }
        private void ToggleIsAppPaused(bool fromHotkey) {
            if(fromHotkey) {
                IsAppPaused = !IsAppPaused;
            }
            ShowNotifcation(fromHotkey, "App", IsAppPaused ? "PAUSED":"ACTIVE", IsAppPaused);
        }

        private RelayCommand<bool> _toggleRightClickPasteCommand;
        public ICommand ToggleRightClickPasteCommand {
            get {
                if (_toggleRightClickPasteCommand == null) {
                    _toggleRightClickPasteCommand = new RelayCommand<bool>(ToggleRightClickPaste, CanToggleRightClickPaste);
                }
                return _toggleRightClickPasteCommand;
            }
        }
        private bool CanToggleRightClickPaste(bool fromHotkey) {
            //only allow append mode to activate if app is not paused and only ONE clip is selected
            return !IsAppPaused;
        }
        private void ToggleRightClickPaste(bool fromHotkey) {
            if (fromHotkey) {
                IsRightClickPasteMode = !IsRightClickPasteMode; 
            }
            ShowNotifcation(fromHotkey, "Right-Click Paste Mode", IsRightClickPasteMode ? "ON":"OFF", IsRightClickPasteMode);
        }

        private RelayCommand<bool> _toggleAppendModeCommand;
        public ICommand ToggleAppendModeCommand {
            get {
                if (_toggleAppendModeCommand == null) {
                    _toggleAppendModeCommand = new RelayCommand<bool>(ToggleAppendMode, CanToggleAppendMode);
                }
                return _toggleAppendModeCommand;
            }
        }
        private bool CanToggleAppendMode(bool fromHotkey) {
            //only allow append mode to activate if app is not paused and only ONE clip is selected
            return !IsAppPaused;
        }
        private void ToggleAppendMode(bool fromHotkey) {
            if (fromHotkey) {
                IsInAppendMode = !IsInAppendMode;
            }
            ShowNotifcation(fromHotkey, "Append Mode", IsInAppendMode ? "ON" : "OFF",IsInAppendMode);
        }

        private RelayCommand<bool> _toggleAutoCopyModeCommand;
        public ICommand ToggleAutoCopyModeCommand {
            get {
                if (_toggleAutoCopyModeCommand == null) {
                    _toggleAutoCopyModeCommand = new RelayCommand<bool>(ToggleAutoCopyMode, CanToggleAutoCopyMode);
                }
                return _toggleAutoCopyModeCommand;
            }
        }
        private bool CanToggleAutoCopyMode(bool fromHotkey) {
            return !IsAppPaused;
        }
        private void ToggleAutoCopyMode(bool fromHotkey) {
            if (fromHotkey) {
                IsAutoCopyMode = !IsAutoCopyMode;
            }
            ShowNotifcation(fromHotkey, "Auto-Copy Mode", IsAutoCopyMode ? "ON" : "OFF",IsAutoCopyMode);
        }
        #endregion
    }
}
