using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media.Animation;
using GalaSoft.MvvmLight.CommandWpf;
using Gma.System.MouseKeyHook;
using Hardcodet.Wpf.TaskbarNotification;

namespace MpWpfApp {
    public class MpMainWindowViewModel : MpViewModelBase {
        #region Statics
        public static bool IsOpen {
            get {
                return Application.Current.MainWindow.Visibility == Visibility.Visible;
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
            SearchBoxViewModel = new MpSearchBoxViewModel() { PlaceholderText = Properties.Settings.Default.SearchPlaceHolderText };
            ClipTrayViewModel = new MpClipTrayViewModel();
            ClipTileSortViewModel = new MpClipTileSortViewModel();
            AppModeViewModel = new MpAppModeViewModel();
            TagTrayViewModel = new MpTagTrayViewModel();
            SystemTrayViewModel = new MpSystemTrayViewModel();
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

        public override bool InitHotkeys() {
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
            //IsOpen = true;

            if (Application.Current.MainWindow == null) {
                Application.Current.MainWindow = new MpMainWindow();
            }
            SetupMainWindowRect();            

            var mw = (MpMainWindow)Application.Current.MainWindow;
            mw.Show();
            mw.Activate();
            mw.Visibility = Visibility.Visible;
            mw.Topmost = true;

            ClipTrayViewModel.ResetClipSelection();

            MpHelpers.AnimateDoubleProperty(
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
            return Application.Current.MainWindow != null && 
                   Application.Current.MainWindow.Visibility == Visibility.Visible &&
                   IsShowingDialog == false &&
                   !MpTemplateTokenEditModalWindowViewModel.IsOpen &&
                   !MpTemplateTokenPasteModalWindowViewModel.IsOpen;
        }
        private async void HideWindow(bool pasteSelected) {
            //IsOpen = false;

            IDataObject pasteDataObject = null;
            if(pasteSelected) {
                pasteDataObject = await ClipTrayViewModel.GetDataObjectFromSelectedClips(pasteSelected);
            }
            var mw = (MpMainWindow)Application.Current.MainWindow;
            MpHelpers.AnimateDoubleProperty(
                _endMainWindowTop,
                _startMainWindowTop,
                Properties.Settings.Default.ShowMainWindowAnimationMilliseconds,
                mw,
                Window.TopProperty,
                (s, e) => {
                    if (pasteSelected) {
                        Console.WriteLine("Pasting " + ClipTrayViewModel.SelectedClipTiles.Count + " items");
                        ClipTrayViewModel.ClipboardManager.PasteDataObject(pasteDataObject);

                        //resort list so pasted items are in front
                        for (int i = ClipTrayViewModel.SelectedClipTiles.Count - 1; i >= 0; i--) {
                            var sctvm = ClipTrayViewModel.SelectedClipTiles[i];
                            ClipTrayViewModel.Move(ClipTrayViewModel.IndexOf(sctvm), 0);
                        }
                    }
                    ClipTrayViewModel.ResetClipSelection();
                    mw.Visibility = Visibility.Collapsed;
                });
        }
        #endregion
    }
}