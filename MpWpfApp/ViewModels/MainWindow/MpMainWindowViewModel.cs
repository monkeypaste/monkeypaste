using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Media3D;
using System.Windows.Threading;
using DataGridAsyncDemoMVVM.filtersort;
using GalaSoft.MvvmLight.CommandWpf;
using Gma.System.MouseKeyHook;
using Hardcodet.Wpf.TaskbarNotification;

namespace MpWpfApp {
    public class MpMainWindowViewModel : MpViewModelBase, IDisposable {
        #region Statics
        public static bool IsMainWindowOpen {
            get {
                return Application.Current.MainWindow.DataContext != null && 
                    Application.Current.MainWindow.Visibility == Visibility.Visible &&
                    ((MpMainWindowViewModel)Application.Current.MainWindow.DataContext).MainWindowGridTop < SystemParameters.WorkArea.Bottom; //Properties.Settings.Default.MainWindowStartHeight;
            }
        }

        public static bool IsShowingMainWindow { get; set; } = false;

        public static bool IsApplicationLoading { get; set; } = true;
        //public static bool IsMainWindowOpen { get; private set; } = false;
        #endregion

        #region Private Variables
        private double _startMainWindowTop;
        private double _endMainWindowTop;
        private double _deltaHeight = 0;

        private List<string> _tempFilePathList { get; set; } = new List<string>();
        #endregion

        #region Public Variables

        public bool IsShowingDialog = false;

        #endregion

        #region Properties       

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

        #region Controls
        public Canvas MainWindowCanvas { get; set; }

        public Grid MainWindowGrid { get; set; }
        #endregion

        #region State
        private bool _isMainWindowLocked = false;
        public bool IsMainWindowLocked {
            get {
                return _isMainWindowLocked;
            }
            set {
                if(_isMainWindowLocked != value) {
                    _isMainWindowLocked = value;
                    OnPropertyChanged(nameof(IsMainWindowLocked));
                    if(IsMainWindowLocked) {
                        SystemTrayViewModel.ShowLogDialogCommand.Execute(null);
                    }
                }
            }
        }
        #endregion

        #region Layout
        public double AppModeButtonGridWidth {
            get {
                if(ClipTrayViewModel == null || !ClipTrayViewModel.IsAnyTileExpanded) {
                    return MpMeasurements.Instance.AppStateButtonPanelWidth;
                }
                return 0;
            }
        }

        public double MainWindowWidth {
            get {
                return SystemParameters.WorkArea.Width;
            }
        }

        public double MainWindowHeight {
            get {
                return SystemParameters.WorkArea.Height;
            }
        }

        public double MainWindowTop {
            get {
                return 0;
            }
        }

        public double MainWindowBottom {
            get {
                return SystemParameters.WorkArea.Height;
            }
        }

        private double _mainWindowGridTop = SystemParameters.WorkArea.Height;
        public double MainWindowGridTop {
            get {
                return _mainWindowGridTop;
            }
            set {
                if(_mainWindowGridTop != value) {
                    _mainWindowGridTop = value;
                    OnPropertyChanged(nameof(MainWindowGridTop));
                }
            }
        }

        private double _mainWindowGridHeight = MpMeasurements.Instance.MainWindowMinHeight;
        public double MainWindowGridHeight {
            get {
                return _mainWindowGridHeight;
            }
            set {
                if (_mainWindowGridHeight != value) {
                    _mainWindowGridHeight = value;
                    OnPropertyChanged(nameof(MainWindowGridHeight));
                }
            }
        }


        private double _clipTrayHeight = MpMeasurements.Instance.ClipTrayMinHeight;
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

        private Visibility _processinngVisibility = Visibility.Collapsed;
        public Visibility ProcessingVisibility {
            get {
                return _processinngVisibility;
            }
            set {
                if(_processinngVisibility != value) {
                    _processinngVisibility = value;
                    OnPropertyChanged(nameof(ProcessingVisibility));
                    OnPropertyChanged(nameof(AppVisibility));
                }
            }
        }


        public Visibility AppVisibility {
            get {
                return ProcessingVisibility == Visibility.Visible ? Visibility.Hidden : Visibility.Visible;
            }
        }
        #endregion

        #region Public Methods        
        public MpMainWindowViewModel() : base() {
            MpMainWindowViewModel.IsApplicationLoading = true;

            MpHelpers.Instance.Init();

            MpPluginManager.Instance.Init();

            if (string.IsNullOrEmpty(Properties.Settings.Default.ThisClientGuid)) {
                Properties.Settings.Default.ThisClientGuid = Guid.NewGuid().ToString();                
            }

            if (MpUserDevice.GetUserDeviceByGuid(Properties.Settings.Default.ThisClientGuid) == null) {
                new MpUserDevice(Properties.Settings.Default.ThisClientGuid, MpUserDeviceType.Windows).WriteToDatabase();
            }

            SystemTrayViewModel = new MpSystemTrayViewModel();
            SearchBoxViewModel = new MpSearchBoxViewModel() { PlaceholderText = Properties.Settings.Default.SearchPlaceHolderText };
            ClipTrayViewModel = new MpClipTrayViewModel();
            ClipTileSortViewModel = new MpClipTileSortViewModel();
            AppModeViewModel = new MpAppModeViewModel();
            TagTrayViewModel = new MpTagTrayViewModel(ClipTrayViewModel);
        }

        public void MainWindow_Loaded(object sender, RoutedEventArgs e) {
            var mw = (MpMainWindow)Application.Current.MainWindow;
            MainWindowCanvas = (Canvas)mw.FindName("MainWindowCanvas");
            MainWindowGrid = (Grid)mw.FindName("MainWindowGrid");

            InitDefaultProperties();

            mw.Deactivated += (s, e2) => {
                HideWindowCommand.Execute(null);
            };
            ClipTrayViewModel.ItemsVisibilityChanged += (s1, e7) => {
                if (ClipTrayViewModel.VisibileClipTiles.Count == 0 && 
                    SearchBoxViewModel.HasText) {
                    SearchBoxViewModel.IsTextValid = false;
                } else {
                    SearchBoxViewModel.IsTextValid = true;
                }
            };

            MainWindowCanvas.MouseLeftButtonDown += (s, e3) => {
                var hitTest = VisualTreeHelper.HitTest(MainWindowCanvas, e3.GetPosition(MainWindowCanvas));
                if(hitTest != null && hitTest.VisualHit != null) {
                    if(hitTest.VisualHit == MainWindowCanvas) {
                        HideWindowCommand.Execute(null);
                    }
                }
                
            };

            SetupMainWindowRect();

            InitWindowStyle();

            MpShortcutCollectionViewModel.Instance.Init();

#if DEBUG
            //ShowWindowCommand.Execute(null);
            //HideWindowCommand.Execute(null);
#else
            //HideWindowCommand.Execute(null);
#endif
            var taskbarIcon = (TaskbarIcon)mw.FindName("TaskbarIcon");
            MpSoundPlayerGroupCollectionViewModel.Instance.Init();

            MpStandardBalloonViewModel.ShowBalloon(
                "Monkey Paste",
                "Successfully loaded w/ " + MpCopyItem.GetTotalItemCount() + " items",
                Properties.Settings.Default.AbsoluteResourcesPath + @"/Images/monkey (2).png");

            MpSoundPlayerGroupCollectionViewModel.Instance.PlayLoadedSoundCommand.Execute(null);
            MpMainWindowViewModel.IsApplicationLoading = false;
        }

        public void ClearEdits() {
            TagTrayViewModel.ClearTagEditing();
            ClipTrayViewModel.ClearClipEditing();
        }

        public void ResetTraySelection() {
            if (!SearchBoxViewModel.HasText) {
                TagTrayViewModel.ResetTagSelection();
                ClipTrayViewModel.ResetClipSelection();
            }
        }

        public void ExpandClipTile(MpClipTileViewModel ctvmToExpand) {
            AppModeViewModel.OnPropertyChanged(nameof(AppModeViewModel.AppModeColumnVisibility));
            OnPropertyChanged(nameof(AppModeButtonGridWidth));
            ClipTrayViewModel.IsolateClipTile(ctvmToExpand);

            double maxDelta = MpMeasurements.Instance.MainWindowMaxHeight - MpMeasurements.Instance.MainWindowMinHeight;
            double ctvmDelta = ctvmToExpand.RichTextBoxViewModelCollection.TotalItemHeight - ctvmToExpand.RichTextBoxViewModelCollection.RtbLbScrollViewerHeight;
            if(ctvmToExpand.IsPastingTemplate) {
                ctvmDelta += ctvmToExpand.PasteTemplateToolbarHeight;
            } else if(ctvmToExpand.IsEditingTile) {
                ctvmDelta += ctvmToExpand.EditRichTextBoxToolbarHeight;
            }
            _deltaHeight = Math.Min(maxDelta,ctvmDelta);//MpMeasurements.Instance.MainWindowMinHeight);
            Resize(_deltaHeight);
            ClipTrayViewModel.Resize(ctvmToExpand,
                                    ClipTrayWidth - ctvmToExpand.TileBorderMinWidth - MpMeasurements.Instance.ClipTileExpandedMargin,
                                    _deltaHeight,
                                    ctvmToExpand.IsPastingTemplate ? 0:ctvmToExpand.EditRichTextBoxToolbarHeight);
        }

        public void ShrinkClipTile(MpClipTileViewModel ctvmToShrink) {
            Resize(-_deltaHeight);
            ClipTrayViewModel.Resize(ctvmToShrink,
                -(MainWindowViewModel.ClipTrayWidth - ctvmToShrink.TileBorderMinWidth - MpMeasurements.Instance.ClipTileExpandedMargin),
                -_deltaHeight,
                ctvmToShrink.IsPastingTemplate ? 0:-ctvmToShrink.EditRichTextBoxToolbarHeight);

            ClipTrayViewModel.RestoreVisibleTiles();

            AppModeViewModel.OnPropertyChanged(nameof(AppModeViewModel.AppModeColumnVisibility));
            OnPropertyChanged(nameof(AppModeButtonGridWidth));
        }

        #endregion

        #region Private Methods
        private void Resize(double deltaHeight) {
            var mw = (MpMainWindow)Application.Current.MainWindow;
            MainWindowGridTop -= deltaHeight;
            MainWindowGridHeight += deltaHeight;
            mw.UpdateLayout();
        }

        private void InitDefaultProperties() {
            if (Properties.Settings.Default.DoFindBrowserUrlForCopy) {
                Properties.Settings.Default.UserDefaultBrowserProcessPath = MpHelpers.Instance.GetSystemDefaultBrowserProcessPath();
            }
            Properties.Settings.Default.UserCultureInfoName = CultureInfo.CurrentCulture.Name;
        }

        private void SetupMainWindowRect() {
            var mw = (MpMainWindow)Application.Current.MainWindow;

            mw.Left = SystemParameters.WorkArea.Left;
            //mw.Height = MpMeasurements.Instance.MainWindowMinHeight;
            
            _startMainWindowTop = SystemParameters.WorkArea.Bottom;
            if (SystemParameters.WorkArea.Top == 0) {
                //if taskbar is at the bottom
                mw.Width = SystemParameters.PrimaryScreenWidth;
                _endMainWindowTop = SystemParameters.WorkArea.Height - MpMeasurements.Instance.MainWindowMinHeight;
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

            MainWindowGridTop = _startMainWindowTop;

            OnPropertyChanged(nameof(MainWindowWidth));
            OnPropertyChanged(nameof(MainWindowHeight));
            //Canvas.SetTop(MainWindowGrid, MainWindowGridTop);
        }

        private void InitWindowStyle() {
            WindowInteropHelper wndHelper = new WindowInteropHelper((MpMainWindow)Application.Current.MainWindow);
            int exStyle = (int)WinApi.GetWindowLong(wndHelper.Handle, (int)WinApi.GetWindowLongFields.GWL_EXSTYLE);
            exStyle |= (int)WinApi.ExtendedWindowStyles.WS_EX_TOOLWINDOW;
            WinApi.SetWindowLong(wndHelper.Handle, (int)WinApi.GetWindowLongFields.GWL_EXSTYLE, (IntPtr)exStyle);

            SystemParameters.StaticPropertyChanged += (sender, args) =>
            {
                if (args.PropertyName == nameof(SystemParameters.WorkArea)) {
                    this.Dispatcher.Invoke(() => {
                        SetupMainWindowRect();
                    });
                }
            };
        }

        public void AddTempFile(string fp) {
            if(_tempFilePathList.Contains(fp.ToLower())) {
                return;
            }
            _tempFilePathList.Add(fp.ToLower());
        }
        #endregion

        #region Disposable
        public void Dispose() {
           // MonkeyPaste.MpSyncManager.Instance.Dispose();
           
            foreach (string tfp in _tempFilePathList) {
                if(File.Exists(tfp)) {
                    try {
                        File.Delete(tfp);
                    }
                    catch(Exception ex) {
                        Console.WriteLine("MainwindowViewModel Dispose error deleteing temp file '" + tfp + "' with exception:");
                        Console.WriteLine(ex);
                    }
                }
            }
        }
        #endregion

        #region Commands

        private RelayCommand _undoCommand;
        public ICommand UndoCommand {
            get {
                if (_undoCommand == null) {
                    _undoCommand = new RelayCommand(() => UndoManager.Undo(), () => UndoManager.CanUndo);
                }
                return _undoCommand;
            }
        }

        private ICommand _redoCommand;
        public ICommand RedoCommand {
            get {
                if (_redoCommand == null)
                    _redoCommand = new RelayCommand(() => UndoManager.Redo(), () => UndoManager.CanRedo);
                return _redoCommand;
            }
        }

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
                //Application.Current.MainWindow.Visibility != Visibility.Visible ||
                MpMainWindowViewModel.IsApplicationLoading ||
                !MpSettingsWindowViewModel.IsOpen) && !IsMainWindowOpen && !IsShowingMainWindow;
        }
        private void ShowWindow() {
            IsShowingMainWindow = true;
            if (Application.Current.MainWindow == null) {
                Application.Current.MainWindow = new MpMainWindow();
            }

            SetupMainWindowRect();

            var mw = (MpMainWindow)Application.Current.MainWindow;
            mw.Show();
            mw.Activate();
            mw.Visibility = Visibility.Visible;
            mw.Topmost = true;

            if (MpMainWindowViewModel.IsApplicationLoading) {
                MpMainWindowViewModel.IsApplicationLoading = false;
                ClipTileSortViewModel.SelectedSortType = ClipTileSortViewModel.SortTypes[0];
            } else {
            }

            double tt = Properties.Settings.Default.ShowMainWindowAnimationMilliseconds;
            double fps = 30;
            double dt = ((_endMainWindowTop - _startMainWindowTop) / tt) / (fps / 1000);

            var timer = new DispatcherTimer(DispatcherPriority.Render);
            timer.Interval = TimeSpan.FromMilliseconds(fps);
            
            timer.Tick += (s, e32) => {
                if (MainWindowGridTop > _endMainWindowTop) {
                    MainWindowGridTop += dt;
                } else {
                    MainWindowGridTop = _endMainWindowTop;
                    timer.Stop();
                    IsShowingMainWindow = false;
                    //SearchBoxViewModel.IsTextBoxFocused = true;
                }
            };
            ClipTrayViewModel.AddNewTiles();
            timer.Start();
        }

        private RelayCommand<object> _hideWindowCommand;
        public ICommand HideWindowCommand {
            get {
                if (_hideWindowCommand == null) {
                    _hideWindowCommand = new RelayCommand<object>(HideWindow, CanHideWindow);
                }
                return _hideWindowCommand;
            }
        }
        private bool CanHideWindow(object args) {
            ///return false;
            return (Application.Current.MainWindow != null &&
                   Application.Current.MainWindow.Visibility == Visibility.Visible &&
                   IsShowingDialog == false && !IsMainWindowLocked && IsMainWindowOpen && !IsShowingMainWindow)  || args != null;
        }
        private async void HideWindow(object args) {
            IDataObject pasteDataObject = null;
            bool pasteSelected = false;
            if(args != null) {
                if(args is bool) {
                    pasteSelected = (bool)args;
                } else if(args is IDataObject) {
                    pasteDataObject = (IDataObject)args;
                }
            }
            bool wasMainWindowLocked = IsMainWindowLocked;
            if (pasteSelected) {
                if(ClipTrayViewModel.IsPastingTemplate) {
                    IsMainWindowLocked = true;
                }
                pasteDataObject = await ClipTrayViewModel.GetDataObjectFromSelectedClips(false,true);
            } 

            var mw = (MpMainWindow)Application.Current.MainWindow;

            if(IsMainWindowOpen) {
                double tt = Properties.Settings.Default.ShowMainWindowAnimationMilliseconds;
                double fps = 30;
                double dt = ((_endMainWindowTop - _startMainWindowTop) / tt) / (fps / 1000);

                var timer = new DispatcherTimer(DispatcherPriority.Render);
                timer.Interval = TimeSpan.FromMilliseconds(fps);

                timer.Tick += (s, e32) => {
                    if (MainWindowGridTop < _startMainWindowTop) {
                        MainWindowGridTop -= dt;
                    } else {
                        MainWindowGridTop = _startMainWindowTop;
                        timer.Stop();
                        
                        mw.Visibility = Visibility.Collapsed;
                        if (pasteDataObject != null) {
                            ClipTrayViewModel.PasteDataObject(pasteDataObject);
                        }

                        IsMainWindowLocked = false;
                        if(wasMainWindowLocked) {
                            ShowWindowCommand.Execute(null);
                            IsMainWindowLocked = true;
                        } else {
                            SearchBoxViewModel.IsTextBoxFocused = false;
                        }
                        ResetTraySelection();
                    }
                };
                timer.Start();
            } else if(pasteDataObject != null) {
                ClipTrayViewModel.PasteDataObject(pasteDataObject,true);
            }
        }

        private RelayCommand<object> _toggleMainWindowLockCommand;
        public ICommand ToggleMainWindowLockCommand {
            get {
                if(_toggleMainWindowLockCommand == null) {
                    _toggleMainWindowLockCommand = new RelayCommand<object>(ToggleMainWindowLock);
                }
                return _toggleMainWindowLockCommand;
            }
        }
        private void ToggleMainWindowLock(object args) {
            if(args == null) {
                //only occurs if called outside of ui so toggle value
                IsMainWindowLocked = !IsMainWindowLocked;
            }
            //Do nothing because two-may binding toggles IsMainWindowLocked
        }        
        #endregion
    }
}