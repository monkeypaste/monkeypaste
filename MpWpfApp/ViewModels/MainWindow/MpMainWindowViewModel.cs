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
using GalaSoft.MvvmLight.CommandWpf;
using MonkeyPaste;

namespace MpWpfApp {
    public class MpMainWindowViewModel : MpViewModelBase<object>, IDisposable {
        #region Singleton Definition
        private static readonly Lazy<MpMainWindowViewModel> _Lazy = new Lazy<MpMainWindowViewModel>(() => new MpMainWindowViewModel());
        public static MpMainWindowViewModel Instance { get { return _Lazy.Value; } }

        #endregion

        #region Statics
        public static bool IsMainWindowOpening { get; set; } = false;
        public static bool IsMainWindowClosing { get; set; } = false;

        public static bool IsMainWindowOpen { get; private set; } = false;


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

        public MpSystemTrayViewModel SystemTrayViewModel => MpSystemTrayViewModel.Instance;

        public MpClipTrayViewModel ClipTrayViewModel => MpClipTrayViewModel.Instance;

        public MpTagTrayViewModel TagTrayViewModel => MpTagTrayViewModel.Instance;

        public MpClipTileSortViewModel ClipTileSortViewModel => MpClipTileSortViewModel.Instance;

        public MpSearchBoxViewModel SearchBoxViewModel => MpSearchBoxViewModel.Instance;

        public MpAppModeViewModel AppModeViewModel => MpAppModeViewModel.Instance;

        #endregion

        #region State

        public bool IsMainWindowLoading { get; set; } = true;

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

        public bool IsResizing { get; set; } = false;

        #endregion

        #region Layout
        public double AppModeButtonGridWidth {
            get {
                if(IsMainWindowLoading || MpClipTrayViewModel.Instance == null || !MpClipTrayViewModel.Instance.IsAnyTileExpanded) {
                    return MpMeasurements.Instance.AppStateButtonPanelWidth;
                }
                return 0;
            }
        }

        public double MainWindowWidth { get; set; } = SystemParameters.WorkArea.Width;

        //public double MainWindowHeight {
        //    get {
        //        return TitleMenuHeight + FilterMenuHeight + ClipTrayHeight;
        //    }
        //}

        //public double MainWindowBottom {
        //    get {
        //        return MainWindowTop + MainWindowHeight;
        //    }
        //}

        public Rect MainWindowContainerRect {
            get {
                return SystemParameters.WorkArea;
            }
        }

        private Rect _mainWindowContentRect;
        public Rect MainWindowContentRect {
            get {
                if(_mainWindowContentRect.IsEmpty) {
                    _mainWindowContentRect = new Rect(
                        MainWindowContainerRect.Left,
                        MainWindowContainerRect.Bottom - MpMeasurements.Instance.MainWindowDefaultHeight,
                        MainWindowContainerRect.Width,
                        MpMeasurements.Instance.MainWindowDefaultHeight);
                }
                return _mainWindowContentRect;
            }
            set {
                if(_mainWindowContentRect != value) {
                    _mainWindowContentRect = value;
                    OnPropertyChanged(nameof(MainWindowContentRect));
                }
            }
        }

        public double MainWindowHeight { get; set; } = SystemParameters.WorkArea.Height;

        public double MainWindowBottom { get; set; } = SystemParameters.WorkArea.Height;

        private double _mainWindowGridTop = SystemParameters.PrimaryScreenHeight;
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
                    _clipTrayHeight = Math.Max(0,value);
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
                    _clipTrayWidth = Math.Max(0,value);
                    OnPropertyChanged(nameof(ClipTrayWidth));
                }
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
        public MpMainWindowViewModel() : base(null) {  }

        public async Task Init() {
            MpConsole.WriteLine("MainWindow Init");

            MpSystemTrayViewModel.Instance.Init();
            Application.Current.Resources["SystemTrayViewModel"] = MpSystemTrayViewModel.Instance;

            MonkeyPaste.MpNativeWrapper.Instance.Init(new MpNativeWrapper() {
                IconBuilder = new MpIconBuilder()
            });

            MpHelpers.Instance.Init();

            //MpPluginManager.Instance.Init();

            await MpMouseViewModel.Instance.Init();

            await MpSearchBoxViewModel.Instance.Init();
            Application.Current.Resources["SearchBoxViewModel"] = MpSearchBoxViewModel.Instance;

            await MpClipTrayViewModel.Instance.Init();
            Application.Current.Resources["ClipTrayViewModel"] = MpClipTrayViewModel.Instance;

            await MpClipTileSortViewModel.Instance.Init();
            Application.Current.Resources["ClipTileSortViewModel"] = MpClipTileSortViewModel.Instance;

            await MpTagTrayViewModel.Instance.Init();
            Application.Current.Resources["TagTrayViewModel"] = MpTagTrayViewModel.Instance;
            //OnPropertyChanged(nameof(TagTrayViewModel));

            await MpShortcutCollectionViewModel.Instance.Init();
            Application.Current.Resources["ShortcutCollectionViewModel"] = MpShortcutCollectionViewModel.Instance;

            await MpAppModeViewModel.Instance.Init();
            Application.Current.Resources["AppModeViewModel"] = MpAppModeViewModel.Instance;

            await MpSoundPlayerGroupCollectionViewModel.Instance.Init();
            Application.Current.Resources["SoundPlayerGroupCollectionViewModel"] = MpSoundPlayerGroupCollectionViewModel.Instance;

            //await MpAnalyticItemCollectionViewModel.Instance.Init();
            //Application.Current.Resources["AnalyticItemCollectionViewModel"] = MpAnalyticItemCollectionViewModel.Instance;

            MpMainWindowViewModel.Instance.SetupMainWindowRect();

            IsMainWindowLoading = false;
        }

        public void ClearEdits() {
            MpTagTrayViewModel.Instance.ClearTagEditing();
            MpClipTrayViewModel.Instance.ClearClipEditing();
        }

        public void ResetTraySelection() {
            if (!MpSearchBoxViewModel.Instance.HasText) {
                //TagTrayViewModel.ResetTagSelection();
                MpClipTrayViewModel.Instance.ResetClipSelection();
            }
        }

        public void SetupMainWindowRect() {
            var mw = (MpMainWindow)Application.Current.MainWindow;

            mw.Left = SystemParameters.WorkArea.Left;
            //mw.Height = MpMeasurements.Instance.MainWindowMinHeight;

            _startMainWindowTop = SystemParameters.WorkArea.Bottom;
            if (SystemParameters.WorkArea.Top == 0) {
                //if taskbar is at the bottom
                mw.Width = SystemParameters.PrimaryScreenWidth;
                _endMainWindowTop = SystemParameters.WorkArea.Height - MpMeasurements.Instance.MainWindowDefaultHeight;
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

        public void AddTempFile(string fp) {
            if(_tempFilePathList.Contains(fp.ToLower())) {
                return;
            }
            _tempFilePathList.Add(fp.ToLower());
        }
        #endregion

        #region Disposable
        public override void Dispose() {
            // MonkeyPaste.MpSyncManager.Instance.Dispose();
            base.Dispose();
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

        public ICommand ShowWindowCommand => new RelayCommand(
            async () => {
                IsMainWindowOpening = true;
                await MpHelpers.Instance.RunOnMainThreadAsync(ShowWindow,DispatcherPriority.Render);
            },
            () => {
                return (Application.Current.MainWindow == null ||
                   //Application.Current.MainWindow.Visibility != Visibility.Visible ||
                   MpMainWindowViewModel.Instance.IsMainWindowLoading ||
                   !MpSettingsWindowViewModel.IsOpen) && !IsMainWindowOpen && !IsMainWindowOpening;
            });

        private void ShowWindow() {
            //Ss = MpHelpers.Instance.CopyScreen();
            //MpHelpers.Instance.WriteBitmapSourceToFile(@"C:\Users\tkefauver\Desktop\ss.png", Ss);

            if (Application.Current.MainWindow == null) {
                Application.Current.MainWindow = new MpMainWindow();
            }            

            SetupMainWindowRect();

            MpMessenger.Instance.Send<MpMessageType>(MpMessageType.MainWindowOpening);

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
                    MpMainWindowViewModel.Instance.IsMainWindowLoading = false;
                    IsMainWindowOpening = false;
                    IsMainWindowOpen = true;
                    OnMainWindowShow?.Invoke(this, null);
                    await MpClipTrayViewModel.Instance.AddNewModels();
                }
            };
            
            timer.Start();            
        }

        public ICommand HideWindowCommand => new RelayCommand<object>(
            async (args) => {
                await MpHelpers.Instance.RunOnMainThreadAsync(async()=> { await HideWindow(args); });
            },
            (args) => {
                return ((Application.Current.MainWindow != null &&
                      Application.Current.MainWindow.Visibility == Visibility.Visible &&
                      !IsShowingDialog &&
                      !_isExpanded &&
                      !IsResizing &&
                      !MpClipTrayViewModel.Instance.IsAnyTileItemDragging &&
                      !IsMainWindowClosing &&
                      IsMainWindowOpen &&
                      !IsMainWindowOpening) || args != null);
            });

        private async Task HideWindow(object args) {
            if(IsMainWindowLocked || IsResizing || IsMainWindowClosing) {
                return;
            }
            IsMainWindowClosing = true;

            IDataObject pasteDataObject = null;
            bool pasteSelected = false;
            if(args != null) {
                if(args is bool) {
                    pasteSelected = (bool)args;
                } else if(args is IDataObject) {
                    pasteDataObject = (IDataObject)args;
                }
            }
            string test;
            bool wasMainWindowLocked = IsMainWindowLocked;
            if (pasteSelected) {
                if(MpClipTrayViewModel.Instance.IsAnyPastingTemplate) {
                    IsMainWindowLocked = true;
                }
                pasteDataObject = await MpClipTrayViewModel.Instance.GetDataObjectFromSelectedClips(false,true);
                test = pasteDataObject.GetData(DataFormats.Text).ToString();
                MpConsole.WriteLine("Cb Text: " + test);
            }

            var mw = (MpMainWindow)Application.Current.MainWindow;

            if(IsMainWindowOpen) {
                double tt = MpPreferences.Instance.HideMainWindowAnimationMilliseconds;
                double fps = 30;
                double dt = ((_endMainWindowTop - _startMainWindowTop) / tt) / (fps / 1000);

                var timer = new DispatcherTimer(DispatcherPriority.Render);
                timer.Interval = TimeSpan.FromMilliseconds(fps);

                timer.Tick += async (s, e32) => {
                    if (MainWindowTop < _startMainWindowTop) {
                        MainWindowTop -= dt;
                    } else {
                        MainWindowTop = _startMainWindowTop;
                        timer.Stop();

                        //MpClipTrayViewModel.Instance.ResetClipSelection();
                        mw.Visibility = Visibility.Collapsed;
                        if (pasteDataObject != null) {
                            await MpClipTrayViewModel.Instance.PasteDataObject(pasteDataObject);
                        }

                        IsMainWindowLocked = false;
                        if(wasMainWindowLocked) {
                            ShowWindowCommand.Execute(null);
                            IsMainWindowLocked = true;
                        } else {
                            MpSearchBoxViewModel.Instance.IsTextBoxFocused = false;
                        }
                        IsMainWindowOpen = false;
                        IsMainWindowClosing = false;

                        OnMainWindowHide?.Invoke(this, null);
                    }
                };
                timer.Start();
            } else if(pasteDataObject != null) {
               await MpClipTrayViewModel.Instance.PasteDataObject(pasteDataObject,true);
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