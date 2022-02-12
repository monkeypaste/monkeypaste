using GalaSoft.MvvmLight.CommandWpf;
using Gma.System.MouseKeyHook;
using GongSolutions.Wpf.DragDrop.Utilities;
using MonkeyPaste;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Linq;

namespace MpWpfApp {
    public class MpSidebarViewModel : 
        MpViewModelBase, 
        MpISingletonViewModel<MpSidebarViewModel>,
        MpIResizableViewModel {
        #region Properties

        #region View Models

        public List<MpISidebarItemViewModel> SidebarItemViewModels {
            get {
                if(MpMainWindowViewModel.Instance.IsMainWindowLoading) {
                    return new List<MpISidebarItemViewModel>();
                }
                return new List<MpISidebarItemViewModel> {
                    MpTagTrayViewModel.Instance,
                    MpAnalyticItemCollectionViewModel.Instance,
                    MpActionCollectionViewModel.Instance
                };
            }
        }

        public MpISidebarItemViewModel VisibleSidebar => SidebarItemViewModels.FirstOrDefault(x => x.IsSidebarVisible);

        #endregion

        #region MpIResizableViewModel Implementation

        public bool IsResizing { get; set; } = false;

        public bool CanResize { get; set; } = false;

        #endregion

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

        public bool IsAnySidebarOpen {
            get {
                if(MpMainWindowViewModel.Instance.IsMainWindowLoading) {
                    return false;
                }
                return MpTagTrayViewModel.Instance.IsSidebarVisible ||
                       MpAnalyticItemCollectionViewModel.Instance.IsSidebarVisible ||
                       MpActionCollectionViewModel.Instance.IsSidebarVisible;
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

        public double TotalSidebarWidth => SidebarWidth + MpMeasurements.Instance.AppStateButtonPanelWidth;

        public double SidebarWidth { get; set; }

        #endregion

        #region Layout

        #endregion

        #endregion

        #region Constructors

        private static MpSidebarViewModel _instance;
        public static MpSidebarViewModel Instance => _instance ?? (_instance = new MpSidebarViewModel());

        public async Task Init() {
            await MpHelpers.RunOnMainThreadAsync(() => {
                PropertyChanged += MpAppModeViewModel_PropertyChanged;

                MpMessenger.Register<MpMessageType>(
                    MpMainWindowViewModel.Instance, 
                    ReceivedMainWindowViewModelMessage);

                MpMessenger.Register(
                    MpClipTrayViewModel.Instance,
                    ReceivedClipTrayViewModelMessage);

                OnPropertyChanged(nameof(CanMouseModes));
            });
        }

        public MpSidebarViewModel() : base(null) { }

        #endregion

        #region Public Methods

        public void RefreshState() {
            OnPropertyChanged(nameof(CanAutoCopyMode));
            OnPropertyChanged(nameof(CanRighClickPasteMode));
            OnPropertyChanged(nameof(CanAppendMode));

            UpdateAppendMode();
        }

        public void ToggleVisibility(MpISidebarItemViewModel sivm) {
            if(sivm.IsSidebarVisible) {
                sivm.IsSidebarVisible = false;
            } else {
                SidebarItemViewModels.ForEach(x => x.IsSidebarVisible = false);
            }
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
                case nameof(IsAnySidebarOpen):
                    if(VisibleSidebar == null) {
                        SidebarWidth = 0;
                    } else {
                        SidebarWidth = VisibleSidebar.SidebarWidth;
                        var temp = VisibleSidebar.NextSidebarItem;
                        while(temp != null) {
                            SidebarWidth += temp.SidebarWidth;
                            temp = temp.NextSidebarItem;
                        }
                        
                    }
                    OnPropertyChanged(nameof(TotalSidebarWidth));
                    //if (VisibleSidebar is MpActionCollectionViewModel) {
                    //    CanResize = false;
                    //} else {
                    //    CanResize = true;
                    //}
                    break;
            }
        }

        private void ShowNotifcation(string modeType, string status, bool isOn) {
            if (MpPreferences.NotificationShowModeChangeToast) {
                MpStandardBalloonViewModel.ShowBalloon("Monkey Paste", modeType + " is " + status);
            }
            if (MpPreferences.NotificationDoModeChangeSound) {
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
