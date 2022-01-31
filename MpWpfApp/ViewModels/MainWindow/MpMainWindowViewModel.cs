using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
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
    public class MpMainWindowViewModel : 
        MpViewModelBase,
        MpISingletonViewModel<MpMainWindowViewModel>,
        MpIResizableViewModel {
        #region Statics


        public static void SetLogText(string text, bool append = false) {
            Task.Run(async () => {
                await MpHelpers.RunOnMainThreadAsync(() => {
                    text = text == null ? string.Empty : text;
                    var mwvm = (Application.Current.MainWindow as MpMainWindow).DataContext as MpMainWindowViewModel;
                    mwvm.LogText = append ? mwvm.LogText + text : text;
                });
            });
        }

        #endregion

        #region Private Variables
        private double _lastSearchCriteriaHeight = 0;

        private double _startMainWindowTop;
        private double _endMainWindowTop;

        private List<string> _tempFilePathList { get; set; } = new List<string>();
        #endregion

        #region Public Variables

        public bool IsShowingDialog = false;

        #endregion

        #region Properties       

        #region View Models

        #endregion

        #region State

        public bool IsMainWindowInitiallyOpening { get; set; } = true;

        public bool IsMainWindowOpening { get; set; } = false;

        public bool IsMainWindowClosing { get; set; } = false;

        public bool IsMainWindowOpen { get; private set; } = false;

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

        public bool CanResize { get; set; } = false;

        #endregion

        #region Layout

        //public Rect MainWindowRect { get; set; } = new Rect(0, 0, MpMeasurements.Instance.ScreenWidth, MpMeasurements.Instance.MainWindowDefaultHeight);
        public double MainWindowContainerTop {
            get {
                if(IsMainWindowLocked) {
                    return 20000;
                }
                if(MpDragDropManager.IsDragAndDrop &&
                   MpDragDropManager.DropType == MpDropType.External) {
                    return (double)int.MaxValue;
                }
                return 0;
            }            
        }

        public double MainWindowWidth { get; set; } = MpMeasurements.Instance.ScreenWidth;

        public double MainWindowHeight { get; set; } = MpMeasurements.Instance.MainWindowDefaultHeight;

        public double MainWindowTop { get; set; } = MpMeasurements.Instance.WorkAreaBottom;

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

        private static MpMainWindowViewModel _instance;
        public static MpMainWindowViewModel Instance => _instance ?? (_instance = new MpMainWindowViewModel());


        public MpMainWindowViewModel() : base(null) {
            //MpHelpers.RunOnMainThreadAsync(Init);
        }

        public async Task Init() {
            MpConsole.WriteLine("MainWindow Init");
            PropertyChanged += MpMainWindowViewModel_PropertyChanged;

            SetupMainWindowRect();
            //Application.Current.Resources["MainWindowViewModel"] = this;

            MpMessenger.Register<MpMessageType>(
                MpSearchBoxViewModel.Instance,
                ReceivedSearchBoxViewModelMessage);
            
            //IsMainWindowLoading = false;
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

        public void SetupMainWindowRect(bool isAnimated = true) {
            switch (MpScreenInformation.TaskbarLocation) {
                case MpTaskbarLocation.Bottom:
                    _startMainWindowTop = SystemParameters.WorkArea.Bottom; 
                    _endMainWindowTop =  SystemParameters.WorkArea.Bottom - MainWindowHeight;
                    break;
            }

            MainWindowTop = isAnimated ? _startMainWindowTop:_endMainWindowTop;
        }

        #endregion

        #region Private Methods

        private void MpMainWindowViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            switch(e.PropertyName) {
                case nameof(IsMainWindowLocked):
                    OnPropertyChanged(nameof(MainWindowContainerTop));
                    break;
                case nameof(MainWindowHeight):
                    MpClipTrayViewModel.Instance.OnPropertyChanged(nameof(MpClipTrayViewModel.Instance.ClipTrayHeight));
                    break;
                case nameof(IsResizing):
                    if(!IsResizing) {
                        if(MpClipTrayViewModel.Instance.IsAnyTileExpanded) {
                            MpMessenger.Send<MpMessageType>(MpMessageType.ExpandComplete);
                        } else {
                            MpMessenger.Send<MpMessageType>(MpMessageType.UnexpandComplete);
                        }
                    }
                    break;
            }
        }

        public void AddTempFile(string fp) {
            if(_tempFilePathList.Contains(fp.ToLower())) {
                return;
            }
            _tempFilePathList.Add(fp.ToLower());
        }
        #endregion

        #region Private Methods

        private void ReceivedSearchBoxViewModelMessage(MpMessageType msg) {
            switch(msg) {
                case MpMessageType.SearchCriteriaItemsChanged:
                    double yDiff = MpSearchBoxViewModel.Instance.SearchCriteriaListBoxHeight - _lastSearchCriteriaHeight;

                    MainWindowHeight += yDiff;
                    SetupMainWindowRect(false);

                    _lastSearchCriteriaHeight = MpSearchBoxViewModel.Instance.SearchCriteriaListBoxHeight;
                    //MpClipTrayViewModel.Instance.OnPropertyChanged(nameof(MpClipTrayViewModel.Instance.ClipTrayHeight));
                    break;
            }
        }


        public void ReceivedResizerBehaviorMessage(MpMessageType msg) {
            switch (msg) {
                case MpMessageType.Resizing:
                    //IsResizing = true;

                    MainWindowTop = MpMeasurements.Instance.WorkAreaBottom - MainWindowHeight;
                    
                    break;
                case MpMessageType.ResizeCompleted:
                    //IsResizing = false;
                    MpPreferences.MainWindowInitialHeight = MainWindowHeight;
                    break;
            }
        }

        #endregion

        #region Disposable
        public override void Dispose() {
            // MonkeyPaste.MpSyncManager.Dispose();
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

                //Ss = MpHelpers.CopyScreen();
                //MpHelpers.WriteBitmapSourceToFile(@"C:\Users\tkefauver\Desktop\ss.png", Ss);



                MpMessenger.Send<MpMessageType>(MpMessageType.MainWindowOpening);

                var mw = (MpMainWindow)Application.Current.MainWindow;
                while(mw == null) {
                    await Task.Delay(100);
                    mw = (MpMainWindow)Application.Current.MainWindow;
                }
                mw.Show();
                mw.Activate();
                mw.Visibility = Visibility.Visible;
                mw.Topmost = true;

                if (IsMainWindowInitiallyOpening) {
                    //await MpMainWindowResizeBehavior.Instance.ResizeForInitialLoad();
                    IsMainWindowInitiallyOpening = false;
                }

                SetupMainWindowRect();

                double tt = MpPreferences.ShowMainWindowAnimationMilliseconds;
                double fps = 30;
                double dt = (_endMainWindowTop - _startMainWindowTop) / tt / (fps / 1000);

                var timer = new DispatcherTimer(DispatcherPriority.Normal);
                timer.Interval = TimeSpan.FromMilliseconds(fps);

                timer.Tick += (s, e32) => {
                    if (MainWindowTop > _endMainWindowTop) {
                        MainWindowTop += dt;
                    } else {
                        MainWindowTop = _endMainWindowTop;
                        timer.Stop();
                        IsMainWindowLoading = false;
                        IsMainWindowOpening = false;
                        IsMainWindowOpen = true;
                        OnMainWindowShow?.Invoke(this, null);

                        MpClipTrayViewModel.Instance.AddNewItemsCommand.Execute(null);
                    }
                };

                timer.Start();
            },
            () => {
                return (Application.Current.MainWindow == null ||
                   //Application.Current.MainWindow.Visibility != Visibility.Visible ||
                   !MpMainWindowViewModel.Instance.IsMainWindowLoading ||
                   !MpMainWindowViewModel.Instance.IsShowingDialog) && !IsMainWindowOpen && !IsMainWindowOpening;
            });

        public ICommand HideWindowCommand => new RelayCommand<object>(
            async (args) => {
                if (IsMainWindowLocked || IsResizing || IsMainWindowClosing || IsShowingDialog) {
                    return;
                }

                MpDataObject pasteDataObject = null;
                bool pasteSelected = false;
                if (args != null) {
                    if (args is bool) {
                        pasteSelected = (bool)args;
                    } else if (args is MpDataObject) {
                        pasteDataObject = (MpDataObject)args;
                    }
                }
                string test;
                bool wasMainWindowLocked = IsMainWindowLocked;
                if (pasteSelected) {
                    if (MpClipTrayViewModel.Instance.IsAnyPastingTemplate) {
                        IsMainWindowLocked = true;
                    }
                    if(pasteDataObject == null) {
                        pasteDataObject = await MpClipTrayViewModel.Instance.GetDataObjectFromSelectedClips(false, true);
                    }
                    //test = pasteDataObject.GetData(DataFormats.Text).ToString();
                    //MpConsole.WriteLine("Cb Text: " + test);
                }

                var mw = (MpMainWindow)Application.Current.MainWindow;

                if (IsMainWindowOpen) {
                    IsMainWindowClosing = true;

                    double tt = MpPreferences.HideMainWindowAnimationMilliseconds;
                    double fps = 30;
                    double dt = (_endMainWindowTop - _startMainWindowTop) / tt / (fps / 1000);

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
                            } else if (MpClipTrayViewModel.Instance.IsAnyTileExpanded) {
                                MpClipTrayViewModel.Instance.Items.FirstOrDefault(x => x.IsExpanded).IsExpanded = false;
                            }

                            IsMainWindowLocked = false;
                            if (wasMainWindowLocked) {
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
                } else if (pasteDataObject != null) {
                    await MpClipTrayViewModel.Instance.PasteDataObject(pasteDataObject, true);
                }
            },
            (args) => {
                return ((Application.Current.MainWindow != null &&
                      Application.Current.MainWindow.Visibility == Visibility.Visible &&
                      !IsShowingDialog &&
                      !IsResizing &&
                      !IsShowingDialog &&
                      !IsMainWindowClosing &&
                      IsMainWindowOpen &&
                      !IsMainWindowOpening) || args != null);
            });

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