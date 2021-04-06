using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Animation;
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

        public IKeyboardMouseEvents GlobalHook { get; set; }
        public IKeyboardMouseEvents ApplicationHook { get; set; }

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
                }
            }
        }

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
        #endregion

        #region Layout
        public double AppStateButtonGridWidth {
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
            TagTrayViewModel = new MpTagTrayViewModel(ClipTrayViewModel);
        }

        public void MainWindow_Loaded(object sender, RoutedEventArgs e) {
            var mw = (MpMainWindow)Application.Current.MainWindow;
            MainWindowCanvas = (Canvas)mw.FindName("MainWindowCanvas");
            MainWindowGrid = (Grid)mw.FindName("MainWindowGrid");

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

            InitHotkeys();

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
                "Successfully loaded w/ " + ClipTrayViewModel.Count + " items",
                Properties.Settings.Default.AbsoluteResourcesPath + @"/Images/monkey (2).png");

            MpSoundPlayerGroupCollectionViewModel.Instance.PlayLoadedSoundCommand.Execute(null);

            //MpWordsApiDictionary.Instance.TestWordsGet();
            //for (int i = 0; i < 50; i++) {
            //    ClipTrayViewModel.Add(new MpClipTileViewModel(MpCopyItem.CreateRandomItem(MpCopyItemType.RichText)));
            //}
            IsLoading = false;

            //ClipTrayViewModel.Refresh();
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
            OnPropertyChanged(nameof(AppStateButtonGridWidth));
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
                                    ctvmToExpand.EditRichTextBoxToolbarHeight);

            //EventHandler postFadeEvent = (s, e) => {
            //    //Console.WriteLine("Expanding tile post fade event");
            //    ClipTrayVirtualizingStackPanel.HorizontalAlignment = HorizontalAlignment.Center;

            //    //var listBoxItem = (ListBoxItem)ListBox.ItemContainerGenerator.ContainerFromItem(ctvmToExpand);
            //    //double sx = listBoxItem.TranslatePoint(new Point(0.0, 0.0), ListBox).X;
            //    //_originalExpandedTileX = sx;
            //    //double trayMidX = MainWindowViewModel.ClipTrayWidth / 2;
            //    //double ex = trayMidX - (ctvmToExpand.TileBorderMaxWidth / 2);
            //    ////if (sx > ex) {
            //    ////    ex += sx;
            //    ////}
            //    //var T = ctvmToExpand.ClipBorderTranslateTransform;
            //    //var anim = new DoubleAnimation(sx, ex, new Duration(TimeSpan.FromMilliseconds(animMs)));
            //    ////anim.BeginTime = TimeSpan.FromMilliseconds(animMs);
            //    //anim.Completed += (s1, e1) => {

            //    //};

            //    //T.BeginAnimation(TranslateTransform.XProperty, anim);

            //    if (isPastingTemplate) {
            //        ctvmToExpand.IsPastingTemplateTile = true;
            //        ctvmToExpand.RichTextBoxViewModelCollection.SelectRichTextBoxViewModel(0, false, true);
            //    } else {
            //        if (!ctvmToExpand.IsEditingTile) {
            //            ctvmToExpand.IsEditingTile = true;
            //        }
            //        ctvmToExpand.RichTextBoxViewModelCollection.SelectRichTextBoxViewModel(0, true, false);
            //    }
            //};

            //if (_hiddenTileCanvasList.Count > 0) {
            //    MpHelpers.Instance.AnimateVisibilityChange(
            //        _hiddenTileCanvasList,
            //        Visibility.Collapsed,
            //        postFadeEvent,
            //        animMs);
            //} else {
            //    postFadeEvent.Invoke(this, new EventArgs());
            //}
        }

        public void ShrinkClipTile(MpClipTileViewModel ctvmToShrink) {
            Resize(-_deltaHeight);
            ClipTrayViewModel.Resize(ctvmToShrink,
                -(MainWindowViewModel.ClipTrayWidth - ctvmToShrink.TileBorderMinWidth - MpMeasurements.Instance.ClipTileExpandedMargin),
                -_deltaHeight,
                -ctvmToShrink.EditRichTextBoxToolbarHeight);

            ClipTrayViewModel.RestoreVisibleTiles();

            OnPropertyChanged(nameof(AppStateButtonGridWidth));
            AppModeViewModel.OnPropertyChanged(nameof(AppModeViewModel.AppModeColumnVisibility));
            
            //double animMs = 0;// Properties.Settings.Default.ShowMainWindowAnimationMilliseconds;
            //ClearClipSelection(false);
            //ctvmToShrink.IsSelected = true;
            //if (isPastingTemplate) {
            //    ctvmToShrink.IsPastingTemplate = false;
            //    ctvmToShrink.PasteTemplateToolbarViewModel.InitWithRichTextBox(ctvmToShrink.RichTextBoxViewModelCollection[0].Rtb, true);
            //} else {
            //    if (ctvmToShrink.IsEditingTile) {
            //        ctvmToShrink.IsEditingTile = false;
            //    }
            //    ctvmToShrink.EditRichTextBoxToolbarViewModel.InitWithRichTextBox(ctvmToShrink.RichTextBoxViewModelCollection[0].Rtb, true);
            //}
            //ctvmToShrink.RichTextBoxViewModelCollection.UpdateLayout();



            //var listBoxItem = (ListBoxItem)ListBox.ItemContainerGenerator.ContainerFromItem(ctvmToShrink);
            //double sx = listBoxItem.TranslatePoint(new Point(0.0, 0.0), ListBox).X;
            ////double trayMidX = ListBox.ActualWidth / 2;
            //double tw = ctvmToShrink.TileBorderMinWidth;
            //double ex = Math.Max(((_expandedTileVisibleIdx-1) * tw),0);

            //var T = ctvmToShrink.ClipBorderTranslateTransform;

            //var anim = new DoubleAnimation(sx, ex, new Duration(TimeSpan.FromMilliseconds(animMs)));
            ////anim.BeginTime = TimeSpan.FromMilliseconds(animMs);
            //anim.Completed += (s1, e1) => {                
            //    var _hiddenTileCanvasList = new List<FrameworkElement>();
            //    foreach (var ctvm in _hiddenTiles) {
            //        _hiddenTileCanvasList.Add(ctvm.ClipBorder);
            //    }

            //    if (_hiddenTileCanvasList.Count > 0) {
            //        MpHelpers.Instance.AnimateVisibilityChange(
            //            _hiddenTileCanvasList,
            //            Visibility.Visible,
            //            (s,e)=>Refresh(),
            //            animMs,
            //            animMs);
            //    }
            //};
            //T.BeginAnimation(TranslateTransform.XProperty, anim);
        }
        #endregion

        #region Private Methods
        private void Resize(double deltaHeight) {
            var mw = (MpMainWindow)Application.Current.MainWindow;
            MainWindowGridTop -= deltaHeight;
            MainWindowGridHeight += deltaHeight;
            mw.UpdateLayout();
            //mw.ResizeMode = ResizeMode.CanResize;

            //mw.Height += deltaHeight;
            //mw.Top -= deltaHeight;
            //mw.UpdateLayout();
            //mw.ResizeMode = ResizeMode.NoResize;
            //MpHelpers.Instance.AnimateDoubleProperty(
            //    mw.Top,
            //    mw.Top - deltaHeight,
            //    0.1,
            //    mw,
            //    Window.TopProperty,
            //    (s, e) => {

            //    });

            //MpHelpers.Instance.AnimateDoubleProperty(
            //    mw.Height,
            //    mw.Height + deltaHeight,
            //    0.1,
            //    mw,
            //    Window.TopProperty,
            //    (s, e) => {
            //        ClipTrayViewModel.Refresh();
            //    });
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
            //Canvas.SetTop(MainWindowGrid, MainWindowGridTop);
        }

        private void InitWindowStyle() {
            WindowInteropHelper wndHelper = new WindowInteropHelper((MpMainWindow)Application.Current.MainWindow);
            int exStyle = (int)WinApi.GetWindowLong(wndHelper.Handle, (int)WinApi.GetWindowLongFields.GWL_EXSTYLE);
            exStyle |= (int)WinApi.ExtendedWindowStyles.WS_EX_TOOLWINDOW;
            WinApi.SetWindowLong(wndHelper.Handle, (int)WinApi.GetWindowLongFields.GWL_EXSTYLE, (IntPtr)exStyle);
        }

        private void PasteDataObject(IDataObject pasteDataObject) {
            if (ClipTrayViewModel.IsPastingTemplate) {
                IsMainWindowLocked = false;
            }
            ClipTrayViewModel.PerformPaste(pasteDataObject);
            foreach (var sctvm in ClipTrayViewModel.SelectedClipTiles) {
                if (sctvm.HasTemplate) {
                    //cleanup template by recreating hyperlinks
                    //and reseting tile state
                    sctvm.TileVisibility = Visibility.Visible;
                    sctvm.TemplateRichText = string.Empty;
                    //sctvm.RichTextBoxViewModelCollection.SelectRichTextBoxViewModel(0, false, true);
                    foreach (var rtbvm in sctvm.RichTextBoxViewModelCollection) {
                        rtbvm.TemplateRichText = string.Empty;
                    }
                }
            }
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

                //ApplicationHook.MouseClick += (s, e) => {
                //    if (ClipTrayViewModel.IsPastingTemplate || e.Button != System.Windows.Forms.MouseButtons.Left) {
                //        return;
                //    }
                //    var p = MpHelpers.Instance.GetMousePosition(ClipTrayViewModel.ClipTrayContainerGrid);
                //    var hitTestResult = VisualTreeHelper.HitTest(ClipTrayViewModel.ClipTrayContainerGrid, p)?.VisualHit;
                //    if (hitTestResult == null) {
                //        MainWindowViewModel.ClearEdits();
                //    } else {
                //        var hitTypeName = hitTestResult.ToString();
                //        var hitElement = hitTestResult as FrameworkElement;
                //        if(hitElement == null && hitTypeName != @"MS.Internal.PtsHost.PageVisual") {
                //            return;
                //        }
                //        if(/*hitElement == null ||*/
                //           hitTypeName == @"MS.Internal.PtsHost.PageVisual" ||
                //           hitElement.DataContext == null ||
                //           hitElement.DataContext is MpClipTrayViewModel ||
                //           hitElement.DataContext is MpAppModeViewModel ||
                //           hitElement.DataContext is MpSearchBoxViewModel ||
                //           hitElement.DataContext is MpTagTileViewModel ||
                //           hitElement.DataContext is MpTagTrayViewModel || 
                //           hitElement.DataContext is MpClipTileSortViewModel ||
                //           hitElement.DataContext is MpMainWindowViewModel) {
                //            MainWindowViewModel.ClearEdits();
                //        }
                //    }
                //};

                ApplicationHook.KeyPress += (s, e) => {
                    if (ClipTrayViewModel != null && ClipTrayViewModel.IsAnyTileExpanded) {
                        return;
                    }
                    if (SearchBoxViewModel != null && SearchBoxViewModel.IsTextBoxFocused) {
                        return;
                    }
                    if (TagTrayViewModel != null && TagTrayViewModel.IsEditingTagName) {
                        return;
                    }
                    if (ClipTrayViewModel != null && ClipTrayViewModel.IsEditingClipTitle) {
                        return;
                    }
                    if (MpSettingsWindowViewModel.IsOpen) {
                        return;
                    }
                    if (!char.IsControl(e.KeyChar)) {
                        foreach (var scvm in MpShortcutCollectionViewModel.Instance) {
                        }
                        if (!SearchBoxViewModel.SearchTextBox.IsFocused) {
                            SearchBoxViewModel.SearchTextBox.Text = e.KeyChar.ToString();
                            SearchBoxViewModel.SearchTextBox.Focus();
                        }
                    }
                };

                ApplicationHook.MouseWheel += (s, e) => {
                    if (!MainWindowViewModel.IsLoading && ClipTrayViewModel.IsAnyTileExpanded) {
                        var rtbvm = ClipTrayViewModel.SelectedClipTiles[0].RichTextBoxViewModelCollection;
                        var sv = (ScrollViewer)rtbvm.HostClipTileViewModel.ClipBorder.FindName("ClipTileRichTextBoxListBoxScrollViewer");//RtbLbAdornerLayer.GetVisualAncestor<ScrollViewer>();
                        sv.ScrollToVerticalOffset(sv.VerticalOffset - e.Delta);
                    }
                };

                MpShortcutCollectionViewModel.Instance.Init();
            }
            catch (Exception ex) {
                Console.WriteLine("Error creating mainwindow hotkeys: " + ex.ToString());
                return false;
            }
            return true;
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
           foreach(string tfp in _tempFilePathList) {
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
                Application.Current.MainWindow.Visibility != Visibility.Visible ||
                IsLoading ||
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

            if (IsLoading) {
                IsLoading = false;
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
                }
            };
            timer.Start();
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
            ///return false;
            return ((Application.Current.MainWindow != null &&
                   Application.Current.MainWindow.Visibility == Visibility.Visible &&
                   IsShowingDialog == false && !IsMainWindowLocked) || pasteSelected) && IsMainWindowOpen;
        }
        private async void HideWindow(bool pasteSelected) {
            IDataObject pasteDataObject = null;

            bool wasMainWindowLocked = IsMainWindowLocked;
            if (pasteSelected) {
                if(ClipTrayViewModel.IsPastingTemplate) {
                    IsMainWindowLocked = true;
                }
                pasteDataObject = await ClipTrayViewModel.GetDataObjectFromSelectedClips(pasteSelected);
            } else {
                //ClipTrayViewModel.HideVisibleTiles(500);
            }

            var mw = (MpMainWindow)Application.Current.MainWindow;
            //MpHelpers.Instance.AnimateDoubleProperty(
            //    _endMainWindowTop,
            //    _startMainWindowTop,
            //    Properties.Settings.Default.ShowMainWindowAnimationMilliseconds,
            //    mw,
            //    Window.TopProperty,
            //    (s, e) => {
            //        if (pasteSelected) {
            //            if(ClipTrayViewModel.IsPastingTemplate) {
            //                IsMainWindowLocked = false;
            //            }
            //            ClipTrayViewModel.PerformPaste(pasteDataObject);
            //            foreach (var sctvm in ClipTrayViewModel.SelectedClipTiles) {
            //                if (sctvm.HasTemplate) {
            //                    //cleanup template by recreating hyperlinks
            //                    //and reseting tile state
            //                    sctvm.TileVisibility = Visibility.Visible;
            //                    sctvm.TemplateRichText = string.Empty;
            //                    //sctvm.RichTextBoxViewModelCollection.SelectRichTextBoxViewModel(0, false, true);
            //                    foreach (var rtbvm in sctvm.RichTextBoxViewModelCollection) {
            //                        rtbvm.TemplateRichText = string.Empty;
            //                    }
            //                }
            //            }
            //        }

            //        ResetTraySelection();
                    
            //        mw.Visibility = Visibility.Collapsed;
            //    });
            if(IsMainWindowOpen) {
                double tt = Properties.Settings.Default.ShowMainWindowAnimationMilliseconds;
                double fps = 30;
                double dt = ((_endMainWindowTop - _startMainWindowTop) / tt) / (fps / 1000);

                var timer = new DispatcherTimer(DispatcherPriority.Render);
                timer.Interval = TimeSpan.FromMilliseconds(fps);

                timer.Tick += (s, e32) => {
                    if (MainWindowGridTop < _startMainWindowTop) {
                        MainWindowGridTop -= dt;
                        //Canvas.SetTop(MainWindowGrid, MainWindowGridTop);
                    } else {
                        MainWindowGridTop = _startMainWindowTop;
                        timer.Stop();
                        //IsMainWindowOpen = false;
                        if (pasteSelected) {
                            PasteDataObject(pasteDataObject);
                        }

                        ResetTraySelection();

                        mw.Visibility = Visibility.Collapsed;
                        IsMainWindowLocked = false;
                        if(wasMainWindowLocked) {
                            ShowWindowCommand.Execute(null);
                            IsMainWindowLocked = true;
                        }
                        //mw.WindowState = WindowState.Minimized;
                    }
                };
                timer.Start();
            } else if(pasteDataObject != null) {
                PasteDataObject(pasteDataObject);
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