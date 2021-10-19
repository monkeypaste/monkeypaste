using Microsoft.Toolkit.Mvvm.Input;
using Gma.System.MouseKeyHook;
using GongSolutions.Wpf.DragDrop.Utilities;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace MpWpfApp {
    public class MpAppModeViewModel : MpViewModelBase<object> {
        #region Singleton Definition
        private static readonly Lazy<MpAppModeViewModel> _Lazy = new Lazy<MpAppModeViewModel>(() => new MpAppModeViewModel());
        public static MpAppModeViewModel Instance { get { return _Lazy.Value; } }

        public void Init() { }
        #endregion

        #region Properties

        #region View Models
        #endregion

        public Visibility AppModeColumnVisibility {
            get {
                if(MpMainWindowViewModel.Instance == null || MpMainWindowViewModel.Instance.ClipTrayViewModel == null) {
                    return Visibility.Visible;
                }

                return MpMainWindowViewModel.Instance.ClipTrayViewModel.IsAnyTileExpanded ? Visibility.Collapsed : Visibility.Visible;
            }
        }

        public bool IsAutoAnalysisModeEnabled {
            get {
                return ToggleAutoAnalysisModeCommand.CanExecute(null);
            }
        }

        public bool IsRighClickPasteModeEnabled {
            get {
                return ToggleRightClickPasteCommand.CanExecute(null);
            }
        }

        public bool IsAutoCopyModeEnabled {
            get {
                return ToggleAutoCopyModeCommand.CanExecute(null);
            }
        }

        public bool IsAppendModeEnabled {
            get {
                return ToggleAppendModeCommand.CanExecute(null);
            }
        }

        public string IsAutoAnalysisModeTooltip {
            get {
                string tt = @"Auto Analyze";
                if (!MpMainWindowViewModel.IsMainWindowLoading) {
                    tt += @" " + MpShortcutCollectionViewModel.Instance.GetShortcutViewModelById(25).KeyString;
                }
                return tt;
            }
        }

        public string IsRighClickPasteModeTooltip {
            get {
                string tt = @"Right-Click Paste";
                if (!MpMainWindowViewModel.IsMainWindowLoading) {
                    tt += @" " + MpShortcutCollectionViewModel.Instance.GetShortcutViewModelById(5).KeyString;
                }
                return tt;
            }
        }

        public string IsAutoCopyModeTooltip {
            get {
                string tt = @"Auto Copy Selection";
                if (!MpMainWindowViewModel.IsMainWindowLoading) {
                    tt += @" " + MpShortcutCollectionViewModel.Instance.GetShortcutViewModelById(4).KeyString;
                }
                return tt;
            }
        }

        public string IsAppendModeTooltip {
            get {
                string tt = @"Append Copy";
                if (!MpMainWindowViewModel.IsMainWindowLoading) {
                    tt += @" " + MpShortcutCollectionViewModel.Instance.GetShortcutViewModelById(3).KeyString;
                }
                return tt;
            }
        }

        public string IsAppPausedTooltip {
            get {
                string tt = IsAppPaused ? "Resume":"Pause";
                if (!MpMainWindowViewModel.IsMainWindowLoading) {
                    tt += @" " + MpShortcutCollectionViewModel.Instance.GetShortcutViewModelById(26).KeyString;
                }                
                return tt;
            }
        }


        private bool _isRightClickPasteMode = false;
        public bool IsRightClickPasteMode {
            get {
                return _isRightClickPasteMode;
            }
            set {
                if (_isRightClickPasteMode != value) {
                    _isRightClickPasteMode = value;
                    MonkeyPaste.MpConsole.WriteLine("IsRightClickPasteMode changed to: " + _isRightClickPasteMode);
                    OnPropertyChanged(nameof(IsRightClickPasteMode));
                }
            }
        }

        private bool _isInAutoAnalyzeMode = false;
        public bool IsInAutoAnalyzeMode {
            get {
                return _isInAutoAnalyzeMode;
            }
            set {
                if (_isInAutoAnalyzeMode != value) {
                    _isInAutoAnalyzeMode = value;
                    MonkeyPaste.MpConsole.WriteLine("IsInAutoAnalyzeMode changed to: " + _isInAutoAnalyzeMode);
                    OnPropertyChanged(nameof(IsInAutoAnalyzeMode));
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
                    MonkeyPaste.MpConsole.WriteLine("IsInAppendMode changed to: " + _isInAppendMode);
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
                    OnPropertyChanged(nameof(IsAutoCopyModeEnabled));
                    OnPropertyChanged(nameof(IsRighClickPasteModeEnabled));
                    OnPropertyChanged(nameof(IsAppendModeEnabled));
                    OnPropertyChanged(nameof(IsAutoAnalysisModeEnabled));
                }
            }
        }
        #endregion

        #region Public Methods
        public MpAppModeViewModel() : base(null) {
            PropertyChanged += (s, e) => {
                switch(e.PropertyName) {
                    case nameof(IsAppPaused):
                        ShowNotifcation("App", IsAppPaused ? "PAUSED" : "ACTIVE", IsAppPaused);
                        MpSystemTrayViewModel.Instance.OnPropertyChanged(nameof(MpSystemTrayViewModel.Instance.PauseOrPlayHeader));
                        MpSystemTrayViewModel.Instance.OnPropertyChanged(nameof(MpSystemTrayViewModel.Instance.PauseOrPlayIconSource));
                        break;
                    case nameof(IsRightClickPasteMode):
                        ShowNotifcation("Right-Click Paste Mode", IsRightClickPasteMode ? "ON" : "OFF", IsRightClickPasteMode);
                        break;
                    case nameof(IsInAppendMode):
                        ShowNotifcation("Append Mode", IsInAppendMode ? "ON" : "OFF", IsInAppendMode);
                        UpdateAppendMode();
                        break;
                    case nameof(IsAutoCopyMode):
                        ShowNotifcation("Auto-Copy Mode", IsAutoCopyMode ? "ON" : "OFF", IsAutoCopyMode);
                        break;
                    case nameof(IsInAutoAnalyzeMode):
                        ShowNotifcation("Auto-Analyze Mode", IsInAutoAnalyzeMode ? "ON" : "OFF", IsAutoCopyMode);
                        break;
                }
            };
        }

        public void AppMode_Loaded(object sender, RoutedEventArgs args) {
            var appModeGrid = (Grid)sender;
            var toggleButtonList = appModeGrid.GetVisualDescendents<ToggleButton>();
            foreach(var tb in toggleButtonList) {
                tb.MouseEnter += (s, e) => {
                    OnPropertyChanged(nameof(IsAppendModeTooltip));
                    OnPropertyChanged(nameof(IsAppPausedTooltip));
                    OnPropertyChanged(nameof(IsRighClickPasteModeTooltip));
                    OnPropertyChanged(nameof(IsAutoCopyModeTooltip));
                    OnPropertyChanged(nameof(IsAutoAnalysisModeTooltip));
                };
            }
        }

        public void RefreshState() {
            OnPropertyChanged(nameof(IsAutoCopyModeEnabled));
            OnPropertyChanged(nameof(IsRighClickPasteModeEnabled));
            OnPropertyChanged(nameof(IsAppendModeEnabled));
            OnPropertyChanged(nameof(IsAutoAnalysisModeEnabled));

            UpdateAppendMode();
        }
        #endregion

        #region Private Methods
        private void ShowNotifcation(string modeType, string status, bool isOn) {
            if (Properties.Settings.Default.NotificationShowModeChangeToast) {
                MpStandardBalloonViewModel.ShowBalloon("Monkey Paste", modeType + " is " + status);
            }
            if (Properties.Settings.Default.NotificationDoModeChangeSound) {
                MpSoundPlayerGroupCollectionViewModel.Instance.PlayModeChangeCommand.Execute(isOn);
            }
        }

        private void UpdateAppendMode() {
            if (IsInAppendMode &&
               MpMainWindowViewModel.Instance.ClipTrayViewModel.SelectedItems.Count == 1 &&
               MpMainWindowViewModel.Instance.ClipTrayViewModel.SelectedItems[0] != MpMainWindowViewModel.Instance.ClipTrayViewModel.VisibleItems[0]) {
                int selectedIdx = MpMainWindowViewModel.Instance.ClipTrayViewModel.Items.IndexOf(MpMainWindowViewModel.Instance.ClipTrayViewModel.SelectedItems[0]);
                MpMainWindowViewModel.Instance.ClipTrayViewModel.Items.Move(selectedIdx, 0);
            }
        }
        #endregion

        #region Commands
        private RelayCommand _toggleIsAppPausedCommand;
        public ICommand ToggleIsAppPausedCommand {
            get {
                if (_toggleIsAppPausedCommand == null) {
                    _toggleIsAppPausedCommand = new RelayCommand(ToggleIsAppPaused);
                }
                return _toggleIsAppPausedCommand;
            }
        }
        private void ToggleIsAppPaused() {
            IsAppPaused = !IsAppPaused;
        }

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
            if(MpMainWindowViewModel.IsMainWindowLoading) {
                return false;
            }
            //only allow append mode to activate if app is not paused and only ONE clip is selected
            return !IsAppPaused && MpMainWindowViewModel.Instance.ClipTrayViewModel.SelectedItems.Count <= 1;
        }
        private void ToggleAppendMode() {
            IsInAppendMode = !IsInAppendMode;
            UpdateAppendMode();
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

        private RelayCommand _toggleAutoAnalysisModeCommand;
        public ICommand ToggleAutoAnalysisModeCommand {
            get {
                if (_toggleAutoAnalysisModeCommand == null) {
                    _toggleAutoAnalysisModeCommand = new RelayCommand(ToggleAutoAnalysisMode, CanToggleAutoAnalysisMode);
                }
                return _toggleAutoAnalysisModeCommand;
            }
        }
        private bool CanToggleAutoAnalysisMode() {
            return !IsAppPaused;
        }
        private void ToggleAutoAnalysisMode() {
            IsInAutoAnalyzeMode = !IsInAutoAnalyzeMode;
        }
        #endregion
    }
}
