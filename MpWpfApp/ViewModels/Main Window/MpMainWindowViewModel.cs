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
        public static bool IsOpen = false;
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

        private MpShortcutCollectionViewModel _shortcutCollectionViewModel = null;
        public MpShortcutCollectionViewModel ShortcutCollectionViewModel {
            get {
                return _shortcutCollectionViewModel;
            }
            set {
                if(_shortcutCollectionViewModel != value) {
                    _shortcutCollectionViewModel = value;
                    OnPropertyChanged(nameof(ShortcutCollectionViewModel));
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

        public MpMainWindowViewModel() {
            IsLoading = true;
            SearchBoxViewModel = new MpSearchBoxViewModel(this);
            ClipTrayViewModel = new MpClipTrayViewModel(this);
            ClipTileSortViewModel = new MpClipTileSortViewModel(this);
            AppModeViewModel = new MpAppModeViewModel(this);
            TagTrayViewModel = new MpTagTrayViewModel(this);
            SystemTrayViewModel = new MpSystemTrayViewModel(this);
        }

        public void MainWindow_Loaded(object sender, RoutedEventArgs e) {           
            var mw = (MpMainWindow)Application.Current.MainWindow;
            mw.Deactivated += (s, e2) => {
                HideWindowCommand.Execute(null);
            };
            mw.PreviewKeyDown += MainWindow_PreviewKeyDown;

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
        }

        public void MainWindow_PreviewKeyDown(object sender, KeyEventArgs e) {
            Key key = e.Key;
            if (key == Key.Enter) {
                if (SearchBoxViewModel.IsFocused) {
                    ClipTrayViewModel.SortAndFilterClipTiles();
                } else if (TagTrayViewModel.IsEditingTagName) {
                    TagTrayViewModel.SelectedTagTile.IsEditing = false;
                } else if (ClipTrayViewModel.IsEditingClipTitle) {
                    ClipTrayViewModel.SelectedClipTiles[0].IsEditingTitle = false;
                } else {
                    ClipTrayViewModel.PasteSelectedClipsCommand.Execute(null);
                }
                e.Handled = true;
            } else if ((key == Key.Delete || key == Key.Back) &&
                       !SearchBoxViewModel.IsFocused &&
                       !TagTrayViewModel.IsEditingTagName &&
                       !ClipTrayViewModel.IsEditingClipTitle) {
                //delete clip which shifts focus to neighbor
                ClipTrayViewModel.DeleteSelectedClipsCommand.Execute(null);
                e.Handled = true;
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

                ShortcutCollectionViewModel = new MpShortcutCollectionViewModel(this);                
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

            IsOpen = true;

            var mw = (MpMainWindow)Application.Current.MainWindow;
            mw.Show();
            mw.Activate();
            mw.Visibility = Visibility.Visible;
            mw.Topmost = true;

            ClipTrayViewModel.ResetClipSelection();

            DoubleAnimation ta = new DoubleAnimation();
            ta.From = _startMainWindowTop;
            ta.To = _endMainWindowTop;
            ta.Duration = new Duration(TimeSpan.FromMilliseconds(Properties.Settings.Default.ShowMainWindowAnimationMilliseconds));
            CubicEase easing = new CubicEase();
            easing.EasingMode = EasingMode.EaseIn;
            ta.EasingFunction = easing;
            ta.Completed += (s, e1) => {
                IsLoading = false;
            };
            mw.BeginAnimation(Window.TopProperty, ta);
        }

        private RelayCommand _hideWindowCommand;
        public ICommand HideWindowCommand {
            get {
                if (_hideWindowCommand == null) {
                    _hideWindowCommand = new RelayCommand(HideWindow, CanHideWindow);
                }
                return _hideWindowCommand;
            }
        }
        private bool CanHideWindow() {
            return Application.Current.MainWindow != null && 
                   Application.Current.MainWindow.Visibility == Visibility.Visible &&
                   IsShowingDialog == false;
        }
        private void HideWindow() {
            IsOpen = false;
            var mw = (MpMainWindow)Application.Current.MainWindow;

            DoubleAnimation ta = new DoubleAnimation();
            ta.From = _endMainWindowTop;
            ta.To = _startMainWindowTop;
            ta.Duration = new Duration(TimeSpan.FromMilliseconds(Properties.Settings.Default.ShowMainWindowAnimationMilliseconds));
            ta.Completed += (s, e) => {
                mw.Visibility = Visibility.Collapsed;
                //ShowMainWindowHotKey.Enabled = true;
                //HideMainWindowHotKey.Enabled = false;
                //_clipTrayPhysicsBody.Stop();
            };
            CubicEase easing = new CubicEase();  // or whatever easing class you want
            easing.EasingMode = EasingMode.EaseIn;
            ta.EasingFunction = easing;
            mw.BeginAnimation(Window.TopProperty, ta);
        }
        #endregion
    }
}