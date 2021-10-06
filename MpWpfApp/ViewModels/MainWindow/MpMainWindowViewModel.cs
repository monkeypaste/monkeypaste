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
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Threading;
using AsyncAwaitBestPractices.MVVM;
using DataGridAsyncDemoMVVM.filtersort;
using GalaSoft.MvvmLight.CommandWpf;
using Gma.System.MouseKeyHook;
using Hardcodet.Wpf.TaskbarNotification;
using MonkeyPaste;

namespace MpWpfApp {
    public class MpMainWindowViewModel : MpViewModelBase<object>, IDisposable {
        #region Singleton Definition
        private static readonly Lazy<MpMainWindowViewModel> _Lazy = new Lazy<MpMainWindowViewModel>(() => new MpMainWindowViewModel());
        public static MpMainWindowViewModel Instance { get { return _Lazy.Value; } }

        public void Init() {

        }
        #endregion
        #region Statics
        public static bool IsMainWindowLoading { get; set; } = true;
        public static bool IsMainWindowOpening { get; set; } = false;

        public static bool IsMainWindowOpen { get; private set; } = false;
        //    get {
        //        return Application.Current.MainWindow.DataContext != null && 
        //            Application.Current.MainWindow.Visibility == Visibility.Visible &&
        //            ((MpMainWindowViewModel)Application.Current.MainWindow.DataContext).MainWindowTop < SystemParameters.WorkArea.Bottom; //Properties.Settings.Default.MainWindowStartHeight;
        //    }
        //}


        public static void SetLogText(string text, bool append = false) {
            Task.Run(async () => {
                await MpHelpers.Instance.RunOnMainThreadAsync(() => {
                    text = text == null ? string.Empty : text;
                    var mwvm = (Application.Current.MainWindow as MpMainWindow).DataContext as MpMainWindowViewModel;
                    mwvm.LogText = append ? mwvm.LogText + text : text;
                });
            });
        }

        #endregion

        #region Private Variables
        private double _startMainWindowTop;
        private double _endMainWindowTop;

        private bool _isExpanded = false;

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

        public MpClipTrayViewModel ClipTrayViewModel {
            get {
                return MpClipTrayViewModel.Instance;
            }
        }

        public MpTagTrayViewModel TagTrayViewModel {
            get {
                return MpTagTrayViewModel.Instance;
            }
        }

        public MpClipTileSortViewModel ClipTileSortViewModel {
            get {
                return MpClipTileSortViewModel.Instance;
            }
        }

        public MpSearchBoxViewModel SearchBoxViewModel {
            get {
                return MpSearchBoxViewModel.Instance;
            }
        }

        public MpAppModeViewModel AppModeViewModel {
            get {
                return MpAppModeViewModel.Instance;
            }            
        }

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

        public string LogText { get; set; }

        public BitmapSource Ss { get; set; }

        public ImageBrush SsBrush {
            get {
                if(Ss == null) {
                    return null;
                }
                return new ImageBrush(Ss);
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

        public double MainWindowWidth { get; set; } = SystemParameters.WorkArea.Width;

        public double MainWindowHeight { get; set; } = SystemParameters.WorkArea.Height;

        public double MainWindowBottom { get; set; } = SystemParameters.WorkArea.Height;

        private double _mainWindowGridTop = SystemParameters.WorkArea.Height;
        public double MainWindowTop {
            get {
                return _mainWindowGridTop;
            }
            set {
                if(_mainWindowGridTop != value) {
                    _mainWindowGridTop = value;
                    OnPropertyChanged(nameof(MainWindowTop));
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

        #region Visibility

        private Visibility _processinngVisibility = Visibility.Hidden;
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


        #endregion

        #region Events
        public event EventHandler OnMainWindowShow;
        public event EventHandler OnMainWindowHide;
        #endregion

        #region Public Methods        
        public MpMainWindowViewModel() : base(null) {

            MpSystemTrayViewModel.Instance.Init();
            Application.Current.Resources["SystemTrayViewModel"] = SystemTrayViewModel;
            Task.Run(Initialize);
        }

        private async Task Initialize() {
            MonkeyPaste.MpPreferences.Instance.Init(new MpWpfPreferences());
            await MonkeyPaste.MpDb.Instance.Init(new MpWpfDbInfo());
            MpMainWindowViewModel.IsMainWindowLoading = true;

            MonkeyPaste.MpNativeWrapper.Instance.Init(new MpNativeWrapper() {
                IconBuilder = new MpIconBuilder()
            });

            MpHelpers.Instance.Init();

            //MpPluginManager.Instance.Init();

            MpThemeColors.Instance.Init();

            MpLanguageTranslator.Instance.Init();


            MpSearchBoxViewModel.Instance.Init();
            MpClipTrayViewModel.Instance.Init();
            MpClipTileSortViewModel.Instance.Init();
            MpAppModeViewModel.Instance.Init();
            MpTagTrayViewModel.Instance.Init();

            Application.Current.Resources["ClipTrayViewModel"] = ClipTrayViewModel;
            Application.Current.Resources["TagTrayViewModel"] = TagTrayViewModel;
            Application.Current.Resources["ClipTileSortViewModel"] = ClipTileSortViewModel;
            Application.Current.Resources["SearchBoxViewModel"] = SearchBoxViewModel;
            Application.Current.Resources["AppModeViewModel"] = AppModeViewModel;
        }

        public void FinishLoading() {
            
        }
        public void ClearEdits() {
            TagTrayViewModel.ClearTagEditing();
            ClipTrayViewModel.ClearClipEditing();
        }

        public void ResetTraySelection() {
            if (!SearchBoxViewModel.HasText) {
                //TagTrayViewModel.ResetTagSelection();
                ClipTrayViewModel.ResetClipSelection();
            }
        }

        public void SetupMainWindowRect() {
            var mw = (MpMainWindow)Application.Current.MainWindow;

            mw.Left = SystemParameters.WorkArea.Left;
            //mw.Height = MpMeasurements.Instance.MainWindowMinHeight;

            _startMainWindowTop = SystemParameters.PrimaryScreenHeight;
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

            MainWindowTop = _startMainWindowTop;

            OnPropertyChanged(nameof(MainWindowWidth));
            OnPropertyChanged(nameof(MainWindowHeight));
        }
        #endregion

        #region Private Methods
        private void Resize(double deltaHeight) {
            var mw = (MpMainWindow)Application.Current.MainWindow;
            MainWindowTop -= deltaHeight;
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
                        MonkeyPaste.MpConsole.WriteLine("MainwindowViewModel Dispose error deleteing temp file '" + tfp + "' with exception:");
                        MonkeyPaste.MpConsole.WriteLine(ex);
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

        public IAsyncCommand ShowWindowCommand => new AsyncCommand(
            async () => {
                await MpHelpers.Instance.RunOnMainThreadAsync(ShowWindow,DispatcherPriority.Render);
            },
            (args) => {
                return (Application.Current.MainWindow == null ||
                   //Application.Current.MainWindow.Visibility != Visibility.Visible ||
                   MpMainWindowViewModel.IsMainWindowLoading ||
                   !MpSettingsWindowViewModel.IsOpen) && !IsMainWindowOpen && !IsMainWindowOpening;
            });

        private void ShowWindow() {
            //Ss = MpHelpers.Instance.CopyScreen();
            //MpHelpers.Instance.WriteBitmapSourceToFile(@"C:\Users\tkefauver\Desktop\ss.png", Ss);

            IsMainWindowOpening = true;
            if (Application.Current.MainWindow == null) {
                Application.Current.MainWindow = new MpMainWindow();
            }            

            SetupMainWindowRect();

            var mw = (MpMainWindow)Application.Current.MainWindow;
            mw.Show();
            mw.Activate();
            mw.Visibility = Visibility.Visible;
            mw.Topmost = true;


            double tt = Properties.Settings.Default.ShowMainWindowAnimationMilliseconds;
            double fps = 30;
            double dt = ((_endMainWindowTop - _startMainWindowTop) / tt) / (fps / 1000);

            var timer = new DispatcherTimer(DispatcherPriority.Render);
            timer.Interval = TimeSpan.FromMilliseconds(fps);
            
            timer.Tick += async (s, e32) => {
                if (MainWindowTop > _endMainWindowTop) {
                    MainWindowTop += dt;
                } else {
                    MainWindowTop = _endMainWindowTop;
                    timer.Stop();
                    IsMainWindowOpening = false;
                    if (ClipTrayViewModel.ItemsAdded > 0) {
                        await MpClipTrayViewModel.Instance.RefreshTiles();
                        if(IsMainWindowLoading || MpClipTrayViewModel.Instance.ItemsAdded > 0) {
                            MpClipTrayViewModel.Instance.ResetClipSelection();
                            ClipTrayViewModel.ItemsAdded = 0;
                            IsMainWindowLoading = false;
                            TagTrayViewModel.RefreshAllCounts();
                        }
                    }
                    IsMainWindowOpen = true;
                    OnMainWindowShow?.Invoke(this, null);
                }
            };
            
            timer.Start();
        }

        public IAsyncCommand<object> HideWindowCommand => new AsyncCommand<object>(
            async (args) => {
                await MpHelpers.Instance.RunOnMainThreadAsync(async()=> { await HideWindow(args); });
            },
            (args) => {
                return ((Application.Current.MainWindow != null &&
                      Application.Current.MainWindow.Visibility == Visibility.Visible &&
                      !IsShowingDialog &&
                      !_isExpanded &&
                      IsMainWindowOpen &&
                      !IsMainWindowOpening) || args != null);
            });

        private async Task HideWindow(object args) {
            if(IsMainWindowLocked) {
                return;
            }
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
                if(ClipTrayViewModel.IsAnyPastingTemplate) {
                    IsMainWindowLocked = true;
                }
                pasteDataObject = await ClipTrayViewModel.GetDataObjectFromSelectedClips(false,true);
            }

            OnMainWindowHide?.Invoke(this, null);

            var mw = (MpMainWindow)Application.Current.MainWindow;

            if(IsMainWindowOpen) {
                double tt = Properties.Settings.Default.HideMainWindowAnimationMilliseconds;
                double fps = 30;
                double dt = ((_endMainWindowTop - _startMainWindowTop) / tt) / (fps / 1000);

                var timer = new DispatcherTimer(DispatcherPriority.Render);
                timer.Interval = TimeSpan.FromMilliseconds(fps);

                timer.Tick += (s, e32) => {
                    if (MainWindowTop < _startMainWindowTop) {
                        MainWindowTop -= dt;
                    } else {
                        MainWindowTop = _startMainWindowTop;
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
                        IsMainWindowOpen = false;
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