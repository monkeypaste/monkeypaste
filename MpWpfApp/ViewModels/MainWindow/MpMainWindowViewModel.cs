using System;
using System.Collections.Generic;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media.Animation;
using DataGridAsyncDemoMVVM.filtersort;
using GalaSoft.MvvmLight.CommandWpf;
using Gma.System.MouseKeyHook;
using Hardcodet.Wpf.TaskbarNotification;
using Windows.UI.Core;

namespace MpWpfApp {
    public class MpMainWindowViewModel : MpViewModelBase {
        #region Statics
        public static bool IsOpen {
            get {
                return Application.Current.MainWindow.Visibility == Visibility.Visible && 
                    Application.Current.MainWindow.Top < Properties.Settings.Default.MainWindowStartHeight;
            }
        }
        #endregion

        #region View Models
        private MpSystemTrayViewModel _systemTrayViewModel = null;
        public MpSystemTrayViewModel SystemTrayViewModel {
            get {
                return _systemTrayViewModel;
            }
            set {
                if (_systemTrayViewModel != value) {
                    _systemTrayViewModel = value;
                    OnPropertyChanged(nameof(SystemTrayViewModel));
                }
            }
        }

        private MpClipTrayViewModel _clipTrayViewModel = null;
        public MpClipTrayViewModel ClipTrayViewModel {
            get {
                return _clipTrayViewModel;
            }
            set {
                if (_clipTrayViewModel != value) {
                    _clipTrayViewModel = value;
                    OnPropertyChanged(nameof(ClipTrayViewModel));
                }
            }
        }

        private MpTagTrayViewModel _tagTrayViewModel = null;
        public MpTagTrayViewModel TagTrayViewModel {
            get {
                return _tagTrayViewModel;
            }
            set {
                if (_tagTrayViewModel != value) {
                    _tagTrayViewModel = value;
                    OnPropertyChanged(nameof(TagTrayViewModel));
                }
            }
        }

        private MpClipTileSortViewModel _clipTileSortViewModel = null;
        public MpClipTileSortViewModel ClipTileSortViewModel {
            get {
                return _clipTileSortViewModel;
            }
            set {
                if (_clipTileSortViewModel != value) {
                    _clipTileSortViewModel = value;
                    OnPropertyChanged(nameof(ClipTileSortViewModel));
                }
            }
        }

        private MpSearchBoxViewModel _searchBoxViewModel = null;
        public MpSearchBoxViewModel SearchBoxViewModel {
            get {
                return _searchBoxViewModel;
            }
            set {
                if (_searchBoxViewModel != value) {
                    _searchBoxViewModel = value;
                    OnPropertyChanged(nameof(SearchBoxViewModel));
                }
            }
        }

        private MpAppModeViewModel _appModeViewModel = null;
        public MpAppModeViewModel AppModeViewModel {
            get {
                return _appModeViewModel;
            }
            set {
                if (_appModeViewModel != value) {
                    _appModeViewModel = value;
                    OnPropertyChanged(nameof(AppModeViewModel));
                }
            }
        }

        #endregion

        #region Private Variables
        private double _startMainWindowTop;
        private double _endMainWindowTop;
        #endregion

        #region Public Variables

        public bool IsShowingDialog = false;

        public IKeyboardMouseEvents GlobalHook { get; set; }
        public IKeyboardMouseEvents ApplicationHook { get; set; }
        #endregion

        #region Properties       

        private bool _isLoading = true;
        public bool IsLoading {
            get {
                return _isLoading;
            }
            set {
                if (_isLoading != value) {
                    _isLoading = value;
                    OnPropertyChanged(nameof(IsLoading));
                }
            }
        }

        public double AppStateButtonGridWidth {
            get {
                return MpMeasurements.Instance.AppStateButtonPanelWidth;
            }
        }

        private double _clipTrayHeight = MpMeasurements.Instance.ClipTrayHeight;
        public double ClipTrayHeight {
            get {
                return _clipTrayHeight;
            }
            set {
                if (_clipTrayHeight != value) {
                    _clipTrayHeight = value;
                    OnPropertyChanged(nameof(ClipTrayHeight));
                }
            }
        }

        private double _clipTrayWidth = MpMeasurements.Instance.ClipTrayWidth;
        public double ClipTrayWidth {
            get {
                return _clipTrayWidth;
            }
            set {
                if (_clipTrayWidth != value) {
                    _clipTrayWidth = value;
                    OnPropertyChanged(nameof(ClipTrayWidth));
                }
            }
        }

        public double TitleMenuHeight {
            get {
                return MpMeasurements.Instance.TitleMenuHeight;
            }
        }

        public double FilterMenuHeight {
            get {
                return MpMeasurements.Instance.FilterMenuHeight;
            }
        }

        #endregion

        #region Public Methods        
        public MpMainWindowViewModel() : base() {
            IsLoading = true;
            
            MpHelpers.Instance.Init();

            SystemTrayViewModel = new MpSystemTrayViewModel();
            SearchBoxViewModel = new MpSearchBoxViewModel() { PlaceholderText = Properties.Settings.Default.SearchPlaceHolderText };
            ClipTrayViewModel = new MpClipTrayViewModel();            
            ClipTileSortViewModel = new MpClipTileSortViewModel();
            AppModeViewModel = new MpAppModeViewModel();
            TagTrayViewModel = new MpTagTrayViewModel();
        }

        public void MainWindow_Loaded(object sender, RoutedEventArgs e) {
            var mw = (MpMainWindow)Application.Current.MainWindow;
            mw.Deactivated += (s, e2) => {
                HideWindowCommand.Execute(null);
            };
            ClipTrayViewModel.ItemsVisibilityChanged += (s1, e7) => {
                if(ClipTrayViewModel.VisibileClipTiles.Count == 0 && SearchBoxViewModel.HasText) {
                    SearchBoxViewModel.IsTextValid = false;
                } else {
                    SearchBoxViewModel.IsTextValid = true;
                }
            };
            SetupMainWindowRect();

            InitWindowStyle();

            InitHotkeys();

#if DEBUG
            ShowWindowCommand.Execute(null);
            //HideWindowCommand.Execute(null);
#else
            HideWindowCommand.Execute(null);
#endif
            var taskbarIcon = (TaskbarIcon)mw.FindName("TaskbarIcon");
            MpSoundPlayerGroupCollectionViewModel.Instance.Init();

            //MpWordsApiDictionary.Instance.TestWordsGet();
            //for (int i = 0; i < 50; i++) {
            //    ClipTrayViewModel.Add(new MpClipTileViewModel(MpCopyItem.CreateRandomItem(MpCopyItemType.RichText)));
            //}
        }

        public void ClearEdits() {
            foreach (MpClipTileViewModel clip in ClipTrayViewModel.ClipTileViewModels) {
                clip.IsEditingTile = false;
                clip.IsPastingTemplateTile = false;
                if(clip.DetectedImageObjectCollectionViewModel != null) {
                    foreach (var diovm in clip.DetectedImageObjectCollectionViewModel) {
                        diovm.IsNameReadOnly = true;
                    }
                }
            }
            foreach (var tag in TagTrayViewModel) {
                tag.IsEditing = false;
            }
        }
        #endregion

        #region Private Methods

        private void SetupMainWindowRect() {
            var mw = (MpMainWindow)Application.Current.MainWindow;

            mw.Left = SystemParameters.WorkArea.Left;
            mw.Height = SystemParameters.PrimaryScreenHeight * 0.35;
            _startMainWindowTop = SystemParameters.PrimaryScreenHeight;
            if (SystemParameters.WorkArea.Top == 0) {
                //if taskbar is at the bottom
                mw.Width = SystemParameters.PrimaryScreenWidth;
                _endMainWindowTop = SystemParameters.WorkArea.Height - mw.Height;
            } else if (SystemParameters.WorkArea.Left != 0) {
                //if taskbar is on the right
                mw.Width = SystemParameters.WorkArea.Width;
                _endMainWindowTop = SystemParameters.PrimaryScreenHeight - mw.Height;
            } else if (SystemParameters.WorkArea.Right != SystemParameters.PrimaryScreenWidth) {
                //if taskbar is on the left
                mw.Width = SystemParameters.WorkArea.Width;
                _endMainWindowTop = SystemParameters.WorkArea.Height - mw.Height;
            } else {
                //if taskbar is on the top
                mw.Width = SystemParameters.PrimaryScreenWidth;
                _endMainWindowTop = SystemParameters.PrimaryScreenHeight - mw.Height;
            }
        }

        private void InitWindowStyle() {
            WindowInteropHelper wndHelper = new WindowInteropHelper((MpMainWindow)Application.Current.MainWindow);
            int exStyle = (int)WinApi.GetWindowLong(wndHelper.Handle, (int)WinApi.GetWindowLongFields.GWL_EXSTYLE);
            exStyle |= (int)WinApi.ExtendedWindowStyles.WS_EX_TOOLWINDOW;
            WinApi.SetWindowLong(wndHelper.Handle, (int)WinApi.GetWindowLongFields.GWL_EXSTYLE, (IntPtr)exStyle);

        }

        public bool InitHotkeys() {
            try {
                GlobalHook = Hook.GlobalEvents();
                ApplicationHook = Hook.AppEvents();

                GlobalHook.MouseMove += (s, e) => {
                    if (e.Y <= Properties.Settings.Default.ShowMainWindowMouseHitZoneHeight) {
                        if (ShowWindowCommand.CanExecute(null)) {
                            ShowWindowCommand.Execute(null);
                        }
                    }
                };

                ApplicationHook.KeyPress += (s, e) => {
                    if (!char.IsControl(e.KeyChar)) {
                        foreach(var scvm in MpShortcutCollectionViewModel.Instance) {
                            if()
                        }
                        if(!SearchBoxViewModel.GetSearchTextBox().IsFocused) {
                            SearchBoxViewModel.GetSearchTextBox().Text = e.KeyChar.ToString();
                            SearchBoxViewModel.GetSearchTextBox().Focus();
                        }
                    }
                };

                MpShortcutCollectionViewModel.Instance.Init();
            }
            catch(Exception ex) {
                Console.WriteLine("Error creating mainwindow hotkeys: " + ex.ToString());
                return false;
            }
            return true;
        }
        #endregion

        #region Commands
        private RelayCommand _showWindowCommand;
        public ICommand ShowWindowCommand {
            get {
                if (_showWindowCommand == null) {
                    _showWindowCommand = new RelayCommand(ShowWindow, CanShowWindow);
                }
                return _showWindowCommand;
            }
        }
        private bool CanShowWindow() {
            return (Application.Current.MainWindow == null || 
                Application.Current.MainWindow.Visibility != Visibility.Visible || 
                IsLoading ||
                !MpSettingsWindowViewModel.IsOpen) && !IsOpen;
        }
        private void ShowWindow() {
            if (Application.Current.MainWindow == null) {
                Application.Current.MainWindow = new MpMainWindow();
            }

            SetupMainWindowRect();            

            var mw = (MpMainWindow)Application.Current.MainWindow;
            mw.Show();
            mw.Activate();
            mw.Visibility = Visibility.Visible;
            mw.Topmost = true;            

            if(!IsLoading) {
                ClipTrayViewModel.ResetClipSelection();
            }

            MpHelpers.Instance.AnimateDoubleProperty(
                _startMainWindowTop,
                _endMainWindowTop,
                Properties.Settings.Default.ShowMainWindowAnimationMilliseconds,
                mw,
                Window.TopProperty,
                (s,e) => {
                    IsLoading = false;
                });
        }

        private RelayCommand<bool> _hideWindowCommand;
        public ICommand HideWindowCommand {
            get {
                if (_hideWindowCommand == null) {
                    _hideWindowCommand = new RelayCommand<bool>(HideWindow, CanHideWindow);
                }
                return _hideWindowCommand;
            }
        }
        private bool CanHideWindow(bool pasteSelected) {
            //return false;
            return (Application.Current.MainWindow != null && 
                   Application.Current.MainWindow.Visibility == Visibility.Visible &&
                   IsShowingDialog == false) || pasteSelected;
        }
        private async void HideWindow(bool pasteSelected) {
            IDataObject pasteDataObject = null;
            if(pasteSelected) {
                pasteDataObject = await ClipTrayViewModel.GetDataObjectFromSelectedClips(pasteSelected);
            }

            var mw = (MpMainWindow)Application.Current.MainWindow;
            MpHelpers.Instance.AnimateDoubleProperty(
                _endMainWindowTop,
                _startMainWindowTop,
                Properties.Settings.Default.ShowMainWindowAnimationMilliseconds,
                mw,
                Window.TopProperty,
                (s, e) => {
                    if(pasteSelected) {
                        ClipTrayViewModel.PerformPaste(pasteDataObject);
                    }
                    TagTrayViewModel.ResetTagSelection();
                    ClipTrayViewModel.ResetClipSelection();

                    mw.Visibility = Visibility.Collapsed;
                });
        }
        #endregion
    }
}