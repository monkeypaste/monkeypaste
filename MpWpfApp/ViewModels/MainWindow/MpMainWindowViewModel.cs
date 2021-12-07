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
    public class MpMainWindowViewModel : MpViewModelBase<object> {
        #region Singleton Definition
        private static readonly Lazy<MpMainWindowViewModel> _Lazy = new Lazy<MpMainWindowViewModel>(() => new MpMainWindowViewModel());
        public static MpMainWindowViewModel Instance { get { return _Lazy.Value; } }

        #endregion

        #region Statics


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

        #endregion

        #region Layout

        //public Rect MainWindowRect { get; set; } = new Rect(0, 0, MpMeasurements.Instance.ScreenWidth, MpMeasurements.Instance.MainWindowDefaultHeight);
        public double MainWindowContainerTop {
            get {
                if(IsMainWindowLocked) {
                    return 20000;
                }
                if(MpDragDropManager.Instance.IsDragAndDrop &&
                   MpDragDropManager.Instance.DropType == MpDropType.External) {
                    return (double)int.MaxValue;
                }
                return 0;
            }            
        }

        public double MainWindowWidth { get; set; } = MpMeasurements.Instance.ScreenWidth;

        public double MainWindowHeight { get; set; } = MpMeasurements.Instance.MainWindowDefaultHeight;
                        
        public double MainWindowTop { get; set; } = SystemParameters.WorkArea.Bottom;

        public double ClipTrayAndCriteriaListHeight {
            get {
                return MpClipTrayViewModel.Instance.ClipTrayHeight + MpSearchBoxViewModel.Instance.SearchCriteriaListBoxHeight;
            }
        }
        //public double ClipTrayWidth {
        //    get {
        //        return MainWindowWidth - AppModeButtonGridWidth;
        //    }
        //}

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
        }

        public async Task Init() {
            MpConsole.WriteLine("MainWindow Init");
            PropertyChanged += MpMainWindowViewModel_PropertyChanged;

            MpDataModelProvider.Instance.Init(new MpWpfQueryInfo());

            await MpSystemTrayViewModel.Instance.Init();
            Application.Current.Resources["SystemTrayViewModel"] = MpSystemTrayViewModel.Instance;

            MonkeyPaste.MpNativeWrapper.Instance.Init(new MpNativeWrapper() {
                IconBuilder = new MpIconBuilder()
            });

            await MpHelpers.Instance.Init();

            //MpPluginManager.Instance.Init();

            await MpMouseViewModel.Instance.Init();

            await MpSearchBoxViewModel.Instance.Init();
            Application.Current.Resources["SearchBoxViewModel"] = MpSearchBoxViewModel.Instance;
            
            await MpAnalyticItemCollectionViewModel.Instance.Init();
            Application.Current.Resources["AnalyticItemCollectionViewModel"] = MpAnalyticItemCollectionViewModel.Instance;

            await MpClipTrayViewModel.Instance.Init();
            Application.Current.Resources["ClipTrayViewModel"] = MpClipTrayViewModel.Instance;

            await MpClipTileSortViewModel.Instance.Init();
            Application.Current.Resources["ClipTileSortViewModel"] = MpClipTileSortViewModel.Instance;

            await MpTagTrayViewModel.Instance.Init();
            Application.Current.Resources["TagTrayViewModel"] = MpTagTrayViewModel.Instance;

            await MpShortcutCollectionViewModel.Instance.Init();
            Application.Current.Resources["ShortcutCollectionViewModel"] = MpShortcutCollectionViewModel.Instance;

            await MpAppModeViewModel.Instance.Init();
            Application.Current.Resources["AppModeViewModel"] = MpAppModeViewModel.Instance;

            await MpSoundPlayerGroupCollectionViewModel.Instance.Init();
            Application.Current.Resources["SoundPlayerGroupCollectionViewModel"] = MpSoundPlayerGroupCollectionViewModel.Instance;
                        
            MpMainWindowViewModel.Instance.SetupMainWindowRect();

            MpClipTrayViewModel.Instance.RequeryCommand.Execute(null);

            while(MpClipTrayViewModel.Instance.IsBusy) { await Task.Delay(100); }

            int totalItems = await MpDataModelProvider.Instance.GetTotalCopyItemCountAsync();
            MpStandardBalloonViewModel.ShowBalloon(
                    "Monkey Paste",
                    "Successfully loaded w/ " + totalItems + " items",
                    Properties.Settings.Default.AbsoluteResourcesPath + @"/Images/monkey (2).png");

            Application.Current.Resources["MainWindowViewModel"] = MpMainWindowViewModel.Instance;

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
            switch (MpScreenInformation.TaskbarLocation) {
                case MpTaskbarLocation.Bottom:
                    _startMainWindowTop = SystemParameters.WorkArea.Bottom; 
                    _endMainWindowTop =  SystemParameters.WorkArea.Bottom - MainWindowHeight;
                    break;
            }

            MainWindowTop = _startMainWindowTop;
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
                    //MpClipTrayViewModel.Instance.OnPropertyChanged(nameof(MpClipTrayViewModel.Instance.ClipTrayScreenHeight));
                    OnPropertyChanged(nameof(ClipTrayAndCriteriaListHeight));
                    break;
                case nameof(ClipTrayAndCriteriaListHeight):
                    //if(!IsResizing) {
                    //    MainWindowHeight = MpMeasurements.Instance.TitleMenuHeight +
                    //                    MpMeasurements.Instance.FilterMenuHeight +
                    //                    ClipTrayAndCriteriaListHeight;

                    //    MainWindowTop -= MainWindowHeight - _lastMainWindowHeight;
                    //    OnPropertyChanged(nameof(ClipTrayHeight));
                    //}
                    break;
                case nameof(MainWindowTop):
                    if(IsResizing) {
                        //double topDelta = MainWindowTop - _lastMainWindowTop;
                        //MpConsole.WriteLine("-----------------------");
                        //MpConsole.WriteLine($"Last Tray Height: {ClipTrayHeight}");
                        //ClipTrayHeight -= topDelta;
                        //ClipTrayViewModel.ExpandedTile.TileBorderHeight -= topDelta;
                        //ClipTrayViewModel.ExpandedTile.TileContentHeight -= topDelta;
                        //OnPropertyChanged(nameof())
                    }
                    //OnPropertyChanged(nameof(MainWindowVisibleHeight));
                    
                    break;
                case nameof(IsResizing):
                    if(!IsResizing) {
                        if(MpClipTrayViewModel.Instance.IsAnyTileExpanded) {
                            MpMessenger.Instance.Send<MpMessageType>(MpMessageType.ExpandComplete);
                        } else {
                            MpMessenger.Instance.Send<MpMessageType>(MpMessageType.UnexpandComplete);
                        }
                    }
                    break;
                //case nameof(MainWindowRect):
                //    OnPropertyChanged(nameof(MainWindowTop));
                //    OnPropertyChanged(nameof(MainWindowHeight));
                //    OnPropertyChanged(nameof(MainWindowWidth));
                //    break;
            }
        }

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

            //if (Application.Current.MainWindow == null) {
            //    Application.Current.MainWindow = new MpMainWindow();
            //}            

            SetupMainWindowRect();

            MpMessenger.Instance.Send<MpMessageType>(MpMessageType.MainWindowOpening);

            var mw = (MpMainWindow)Application.Current.MainWindow;
            mw.Show();
            mw.Activate();
            mw.Visibility = Visibility.Visible;
            mw.Topmost = true;

            //MainWindowTop = _endMainWindowTop;
            //IsMainWindowLoading = false;
            //IsMainWindowOpening = false;
            //IsMainWindowOpen = true;
            //OnMainWindowShow?.Invoke(this, null);

            //MpClipTrayViewModel.Instance.AddNewItemsCommand.Execute(null);

            //return;
            double tt = Properties.Settings.Default.ShowMainWindowAnimationMilliseconds;
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
        }

        public ICommand HideWindowCommand => new RelayCommand<object>(
            async (args) => {
                if (IsMainWindowLocked || IsResizing || IsMainWindowClosing) {
                    return;
                }
                IsMainWindowClosing = true;

                IDataObject pasteDataObject = null;
                bool pasteSelected = false;
                if (args != null) {
                    if (args is bool) {
                        pasteSelected = (bool)args;
                    } else if (args is IDataObject) {
                        pasteDataObject = (IDataObject)args;
                    }
                }
                string test;
                bool wasMainWindowLocked = IsMainWindowLocked;
                if (pasteSelected) {
                    if (MpClipTrayViewModel.Instance.IsAnyPastingTemplate) {
                        IsMainWindowLocked = true;
                    }
                    pasteDataObject = await MpClipTrayViewModel.Instance.GetDataObjectFromSelectedClips(false, true);
                    test = pasteDataObject.GetData(DataFormats.Text).ToString();
                    MpConsole.WriteLine("Cb Text: " + test);
                }

                var mw = (MpMainWindow)Application.Current.MainWindow;

                if (IsMainWindowOpen) {
                    double tt = MpPreferences.Instance.HideMainWindowAnimationMilliseconds;
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
                      //!MpContentDropManager.Instance.IsDragAndDrop &&
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