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
using MonkeyPaste;

namespace MpWpfApp {
    public class MpMainWindowViewModel : MpViewModelBase, IDisposable {
        #region Statics
        public static bool IsMainWindowLoading { get; set; } = true;
        public static bool IsMainWindowOpening { get; set; } = false;

        public static bool IsMainWindowOpen {
            get {
                return Application.Current.MainWindow.DataContext != null && 
                    Application.Current.MainWindow.Visibility == Visibility.Visible &&
                    ((MpMainWindowViewModel)Application.Current.MainWindow.DataContext).MainWindowGridTop < SystemParameters.WorkArea.Bottom; //Properties.Settings.Default.MainWindowStartHeight;
            }
        }
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
        public MpSystemTrayViewModel SystemTrayViewModel {
            get {
                return MpSystemTrayViewModel.Instance;
            }
        }

        //private MpClipTrayViewModel _clipTrayViewModel = null;
        public MpClipTrayViewModel ClipTrayViewModel {
            get {
                return MpClipTrayViewModel.Instance;
            }
            //set {
            //    if (_clipTrayViewModel != value) {
            //        _clipTrayViewModel = value;
            //        OnPropertyChanged(nameof(ClipTrayViewModel));
            //    }
            //}
        }

        //private MpTagTrayViewModel _tagTrayViewModel = null;
        public MpTagTrayViewModel TagTrayViewModel {
            get {
                return MpTagTrayViewModel.Instance;
            }
            //set {
            //    if (_tagTrayViewModel != value) {
            //        _tagTrayViewModel = value;
            //        OnPropertyChanged(nameof(TagTrayViewModel));
            //    }
            //}
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

        //private MpSearchBoxViewModel _searchBoxViewModel = null;
        public MpSearchBoxViewModel SearchBoxViewModel {
            get {
                return MpSearchBoxViewModel.Instance;
            }
            //set {
            //    if (_searchBoxViewModel != value) {
            //        _searchBoxViewModel = value;
            //        OnPropertyChanged(nameof(SearchBoxViewModel));
            //    }
            //}
        }

        //private MpAppModeViewModel _appModeViewModel = null;
        public MpAppModeViewModel AppModeViewModel {
            get {
                return MpAppModeViewModel.Instance;
            }            
        }

        #endregion

        #region Controls
        //public Canvas MainWindowCanvas { get; set; }

        //public Grid MainWindowGrid { get; set; }
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
                        MpSystemTrayViewModel.Instance.ShowLogDialogCommand.Execute(null);
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
            //MpViewModelBase.MainWindowViewModel = this;

            MpMainWindowViewModel.IsMainWindowLoading = true;


            MonkeyPaste.MpPreferences.Instance.Init(new MpWpfPreferences());
            MpHelpers.Instance.Init();            

            MonkeyPaste.MpDb.Instance.Init(new MpWpfDbInfo());

            MpPluginManager.Instance.Init();

            MpSystemTrayViewModel.Instance.Init();
            //SearchBoxViewModel = new MpSearchBoxViewModel() {  };
            MpSearchBoxViewModel.Instance.Init();
            MpClipTrayViewModel.Instance.Init();
            //ClipTrayViewModel = new MpClipTrayViewModel();
            ClipTileSortViewModel = new MpClipTileSortViewModel();
            MpAppModeViewModel.Instance.Init();
            MpTagTrayViewModel.Instance.Init();
            //TagTrayViewModel = new MpTagTrayViewModel(ClipTrayViewModel);

            Application.Current.Resources["ClipTrayViewModel"] = ClipTrayViewModel;
            Application.Current.Resources["TagTrayViewModel"] = TagTrayViewModel;
        }

        public void MainWindow_Loaded(object sender, RoutedEventArgs e) {
            MpShortcutCollectionViewModel.Instance.Init();

            MpSoundPlayerGroupCollectionViewModel.Instance.Init();

            int totalItems = MpDb.Instance.GetItems<MpCopyItem>().Count;
            MpStandardBalloonViewModel.ShowBalloon(
                "Monkey Paste",
                "Successfully loaded w/ " + totalItems + " items",
                Properties.Settings.Default.AbsoluteResourcesPath + @"/Images/monkey (2).png");

            MpSoundPlayerGroupCollectionViewModel.Instance.PlayLoadedSoundCommand.Execute(null);

            IsMainWindowLoading = false;
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
            MpAppModeViewModel.Instance.OnPropertyChanged(nameof(MpAppModeViewModel.Instance.AppModeColumnVisibility));
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

            MpAppModeViewModel.Instance.OnPropertyChanged(nameof(MpAppModeViewModel.Instance.AppModeColumnVisibility));
            OnPropertyChanged(nameof(AppModeButtonGridWidth));
        }

        public void SetupMainWindowRect() {
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
        }
        #endregion

        #region Private Methods
        private void Resize(double deltaHeight) {
            var mw = (MpMainWindow)Application.Current.MainWindow;
            MainWindowGridTop -= deltaHeight;
            MainWindowGridHeight += deltaHeight;
            mw.UpdateLayout();
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
                MpMainWindowViewModel.IsMainWindowLoading ||
                !MpSettingsWindowViewModel.IsOpen) && !IsMainWindowOpen && !IsMainWindowOpening;
        }
        private void ShowWindow() {
            IsMainWindowOpening = true;
            if (Application.Current.MainWindow == null) {
                Application.Current.MainWindow = new MpMainWindow();
            }

            if (ClipTrayViewModel.WasItemAdded) {
                Task.Run(() => {
                    MpHelpers.Instance.RunOnMainThread(() => {
                        ClipTrayViewModel.RefreshClips();
                        ClipTrayViewModel.WasItemAdded = false;
                    }, DispatcherPriority.Normal);
                });
            }

            SetupMainWindowRect();

            var mw = (MpMainWindow)Application.Current.MainWindow;
            mw.Show();
            mw.Activate();
            mw.Visibility = Visibility.Visible;
            mw.Topmost = true;

            if (MpMainWindowViewModel.IsMainWindowLoading) {
                MpMainWindowViewModel.IsMainWindowLoading = false;
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
                    IsMainWindowOpening = false;                    
                }
            };            
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
                   IsShowingDialog == false && !IsMainWindowLocked && IsMainWindowOpen && !IsMainWindowOpening)  || args != null;
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