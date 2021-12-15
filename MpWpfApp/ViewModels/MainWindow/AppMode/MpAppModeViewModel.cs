using GalaSoft.MvvmLight.CommandWpf;
using Gma.System.MouseKeyHook;
using GongSolutions.Wpf.DragDrop.Utilities;
using MonkeyPaste;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;

namespace MpWpfApp {
    public class MpAppModeViewModel : MpSingletonViewModel<MpAppModeViewModel> {
        #region Properties

        #region View Models
        #endregion

        public Visibility AppModeColumnVisibility {
            get {
                if(MpMainWindowViewModel.Instance == null || MpClipTrayViewModel.Instance == null) {
                    return Visibility.Visible;
                }

                return MpClipTrayViewModel.Instance.IsAnyTileExpanded ? Visibility.Collapsed : Visibility.Visible;
            }
        }

        #region State

        public bool CanRighClickPasteMode => !IsAppPaused;

        public bool CanAutoCopyMode => !IsAppPaused;

        public bool CanMouseModes => CanAutoCopyMode || CanRighClickPasteMode;

        public bool CanAppendMode => !IsAppPaused;

        private bool _isAutoCopyMode = false;
        public bool IsAutoCopyMode {
            get {
                return _isAutoCopyMode;
            }
            set {
                if (_isAutoCopyMode != value) {
                    _isAutoCopyMode = value;
                    OnPropertyChanged(nameof(IsAutoCopyMode));
                    OnPropertyChanged(nameof(MouseModeImageSourcePath));
                    OnPropertyChanged(nameof(IsAnyMouseModeEnabled));
                }
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
                    OnPropertyChanged(nameof(IsRightClickPasteMode));
                    OnPropertyChanged(nameof(MouseModeImageSourcePath));
                    OnPropertyChanged(nameof(IsAnyMouseModeEnabled));
                }
            }
        }

        public bool IsAnyMouseModeEnabled => IsAutoCopyMode || IsRightClickPasteMode;

        public bool IsAppendMode { get; set; }

        public bool IsAppendLineMode { get; set; }

        public bool IsAnyAppendMode => IsAppendMode || IsAppendLineMode;

        public bool IsGridSplitterEnabled {
            get {
                if(MpMainWindowViewModel.Instance.IsMainWindowLoading) {
                    return false;
                }
                return MpTagTrayViewModel.Instance.IsVisible ||
                       MpAnalyticItemCollectionViewModel.Instance.IsVisible;
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
                    OnPropertyChanged(nameof(CanAutoCopyMode));
                    OnPropertyChanged(nameof(CanRighClickPasteMode));
                    OnPropertyChanged(nameof(CanAppendMode));
                }
            }
        }

        #endregion

        #region Appearance

        #region Tool Tips

        public string IsAutoAnalysisModeTooltip {
            get {
                string tt = @"Auto Analyze";
                if (!MpMainWindowViewModel.Instance.IsMainWindowLoading) {
                    tt += @" " + MpShortcutCollectionViewModel.Instance.GetShortcutViewModelById(25).KeyString;
                }
                return tt;
            }
        }

        public string IsRighClickPasteModeTooltip {
            get {
                string tt = @"Right-Click Paste";
                if (!MpMainWindowViewModel.Instance.IsMainWindowLoading) {
                    tt += @" " + MpShortcutCollectionViewModel.Instance.GetShortcutViewModelById(5).KeyString;
                }
                return tt;
            }
        }

        public string IsAutoCopyModeTooltip {
            get {
                string tt = @"Auto Copy Selection";
                if (!MpMainWindowViewModel.Instance.IsMainWindowLoading) {
                    tt += @" " + MpShortcutCollectionViewModel.Instance.GetShortcutViewModelById(4).KeyString;
                }
                return tt;
            }
        }

        public string IsAppendModeTooltip {
            get {
                string tt = @"Append Copy";
                if (!MpMainWindowViewModel.Instance.IsMainWindowLoading) {
                    tt += @" " + MpShortcutCollectionViewModel.Instance.GetShortcutViewModelById(3).KeyString;
                }
                return tt;
            }
        }

        public string IsAppendLineModeTooltip {
            get {
                string tt = @"Append Line Copy";
                if (!MpMainWindowViewModel.Instance.IsMainWindowLoading) {
                    tt += @" " + MpShortcutCollectionViewModel.Instance.GetShortcutViewModelById(25).KeyString;
                }
                return tt;
            }
        }

        public string IsAppPausedTooltip {
            get {
                string tt = IsAppPaused ? "Resume" : "Pause";
                if (!MpMainWindowViewModel.Instance.IsMainWindowLoading) {
                    tt += @" " + MpShortcutCollectionViewModel.Instance.GetShortcutViewModelById(26).KeyString;
                }
                return tt;
            }
        }

        #endregion

        public string MouseModeImageSourcePath {
            get {
                if(IsRightClickPasteMode && IsAutoCopyMode) {
                    return Application.Current.Resources["BothClickIcon"] as string;
                }
                if (IsRightClickPasteMode) {
                    return Application.Current.Resources["RightClickIcon"] as string;
                }
                if (IsAutoCopyMode) {
                    return Application.Current.Resources["LeftClickIcon"] as string;
                }
                return Application.Current.Resources["NoneClickIcon"] as string;
            }
        }

        #endregion

        #region Layout

        public double DefaultTagTreeWidth => 100;

        public double DefaultAnalyticTreeWidth => 100;

        public double AppModeButtonGridMinWidth {
            get {
                if (MpMainWindowViewModel.Instance.IsMainWindowLoading ||
                   MpClipTrayViewModel.Instance == null ||
                   !MpClipTrayViewModel.Instance.IsAnyTileExpanded) {
                    double ambgw = MpMeasurements.Instance.AppStateButtonPanelWidth;
                    if(MpTagTrayViewModel.Instance.IsVisible) {
                        ambgw += DefaultTagTreeWidth;
                    }
                    if (MpAnalyticItemCollectionViewModel.Instance.IsVisible) {
                        ambgw += DefaultAnalyticTreeWidth;
                    }
                    return ambgw;
                }
                return 0;
            }
        }

        #endregion

        #endregion

        #region Constructors

        public async Task Init() {
            await MpHelpers.Instance.RunOnMainThreadAsync(() => {
                PropertyChanged += MpAppModeViewModel_PropertyChanged;

                MpMessenger.Instance.Register<MpMessageType>(
                    MpMainWindowViewModel.Instance, 
                    ReceivedMainWindowViewModelMessage);

                MpMessenger.Instance.Register(
                    MpClipTrayViewModel.Instance,
                    ReceivedClipTrayViewModelMessage);

                OnPropertyChanged(nameof(CanMouseModes));
            });
        }

        public MpAppModeViewModel() : base() { }

        #endregion

        #region Public Methods

        public void RefreshState() {
            OnPropertyChanged(nameof(CanAutoCopyMode));
            OnPropertyChanged(nameof(CanRighClickPasteMode));
            OnPropertyChanged(nameof(CanAppendMode));

            UpdateAppendMode();
        }
        #endregion

        #region Private Methods

        private void ReceivedClipTrayViewModelMessage(MpMessageType msg) {
            switch (msg) {
                case MpMessageType.TraySelectionChanged:
                    OnPropertyChanged(nameof(CanAppendMode));
                    break;
            }
        }
        
        private void ReceivedMainWindowViewModelMessage(MpMessageType msg) {
            switch(msg) {
                case MpMessageType.UnexpandComplete:
                case MpMessageType.ExpandComplete:
                    OnPropertyChanged(nameof(AppModeButtonGridMinWidth));
                    break;
            }
        }


        private void MpAppModeViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            switch (e.PropertyName) {
                case nameof(IsAppPaused):
                    ShowNotifcation("App", IsAppPaused ? "PAUSED" : "ACTIVE", IsAppPaused);
                    MpSystemTrayViewModel.Instance.OnPropertyChanged(nameof(MpSystemTrayViewModel.Instance.PauseOrPlayHeader));
                    MpSystemTrayViewModel.Instance.OnPropertyChanged(nameof(MpSystemTrayViewModel.Instance.PauseOrPlayIconSource));
                    break;
                case nameof(IsRightClickPasteMode):
                    ShowNotifcation("Right-Click Paste Mode", IsRightClickPasteMode ? "ON" : "OFF", IsRightClickPasteMode);
                    break;
                case nameof(IsAppendMode):
                    ShowNotifcation("Append Mode", IsAppendMode ? "ON" : "OFF", IsAppendMode);
                    UpdateAppendMode();
                    break;
                case nameof(IsAppendLineMode):
                    ShowNotifcation("Append Line Mode", IsAppendLineMode ? "ON" : "OFF", IsAppendLineMode);
                    UpdateAppendMode();
                    break;
                case nameof(IsAutoCopyMode):
                    ShowNotifcation("Auto-Copy Mode", IsAutoCopyMode ? "ON" : "OFF", IsAutoCopyMode);
                    break;
                case nameof(AppModeButtonGridMinWidth):
                    MpClipTrayViewModel.Instance.OnPropertyChanged(nameof(MpClipTrayViewModel.Instance.ClipTrayScreenWidth));
                    break;
            }
        }

        private void ShowNotifcation(string modeType, string status, bool isOn) {
            if (Properties.Settings.Default.NotificationShowModeChangeToast) {
                MpStandardBalloonViewModel.ShowBalloon("Monkey Paste", modeType + " is " + status);
            }
            if (Properties.Settings.Default.NotificationDoModeChangeSound) {
                MpSoundPlayerGroupCollectionViewModel.Instance.PlayModeChangeCommand.Execute(isOn);
            }
        }

        private void UpdateAppendMode() {
            if (IsAnyAppendMode &&
               MpClipTrayViewModel.Instance.SelectedItems.Count == 1 &&
               MpClipTrayViewModel.Instance.SelectedItems[0] != MpClipTrayViewModel.Instance.Items[0]) {
                int selectedIdx = MpClipTrayViewModel.Instance.Items.IndexOf(MpClipTrayViewModel.Instance.SelectedItems[0]);
                MpClipTrayViewModel.Instance.Items.Move(selectedIdx, 0);
            }
        }
        #endregion

        #region Commands

        public ICommand ToggleIsAppPausedCommand => new RelayCommand(
            () => {
                IsAppPaused = !IsAppPaused;
            });

        public ICommand ToggleRightClickPasteCommand => new RelayCommand(
            () => {
                IsRightClickPasteMode = !IsRightClickPasteMode;
            }, CanRighClickPasteMode);

        public ICommand ToggleAutoCopyModeCommand => new RelayCommand(
            () => {
                IsAutoCopyMode = !IsAutoCopyMode;
            }, CanAutoCopyMode);

        public ICommand ToggleAppendModeCommand => new RelayCommand(
            () => {
                IsAppendMode = !IsAppendMode;
                if (IsAppendMode && IsAppendLineMode) {
                    IsAppendLineMode = false;
                }
                UpdateAppendMode();
            },CanAppendMode);

        public ICommand ToggleAppendLineModeCommand => new RelayCommand(
            () => {
                IsAppendLineMode = !IsAppendLineMode;
                if(IsAppendLineMode && IsAppendMode) {
                    IsAppendMode = false;
                }
                UpdateAppendMode();
            }, CanAppendMode);

        #endregion
    }
}
