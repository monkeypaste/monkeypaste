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
using MonkeyPaste.Common.Plugin; using MonkeyPaste.Common; using MonkeyPaste.Common.Wpf;

namespace MpWpfApp {
    public class MpMainWindowViewModel : 
        MpViewModelBase,
        MpIAsyncSingletonViewModel<MpMainWindowViewModel>,
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

        #endregion

        #region Public Variables

        

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

        public bool IsAnyTextBoxFocused { get; set; }

        public bool IsAnyDropDownOpen { get; set; }

        public bool IsShowingDialog { get; set; } = false;

        public bool IsMainWindowActive { get; set; }

        public bool IsFilterMenuVisible { get; set; } = true;

        #endregion

        #region Layout

        //public Rect MainWindowRect { get; set; } = new Rect(0, 0, MpMeasurements.Instance.ScreenWidth, MpMeasurements.Instance.MainWindowDefaultHeight);
        public double MainWindowContainerTop {
            get {
                //if(IsMainWindowLocked) {
                //    return MpMeasurements.Instance.WorkAreaBottom - MainWindowHeight;
                //}
                return 0;
            }            
        }

        public double MainWindowContainerHeight {
            get {
                //if(IsMainWindowLocked) {
                //    return MainWindowHeight;
                //}
                return MpMeasurements.Instance.WorkAreaBottom;
            }
        }

        public double MainWindowWidth { get; set; } = MpMeasurements.Instance.ScreenWidth;

        public double MainWindowHeight { get; set; } = MpMeasurements.Instance.MainWindowDefaultHeight;

        public double MainWindowTop { get; set; } = MpMeasurements.Instance.WorkAreaBottom;

        public double MainWindowTopOpened {
            get {
                switch (MpScreenInformation.TaskbarLocation) {
                    case MpTaskbarLocation.Bottom:
                    default:
                        return SystemParameters.WorkArea.Bottom - MainWindowHeight;
                }
            }
        }

        public double MainWindowTopClosed {
            get {
                switch (MpScreenInformation.TaskbarLocation) {
                    case MpTaskbarLocation.Bottom:
                    default:
                        return SystemParameters.WorkArea.Bottom;
                }
            }
        }

        public double FilterMenuHeight => IsFilterMenuVisible ?
                                            MpMeasurements.Instance.FilterMenuDefaultHeight :
                                            0;
        #endregion

        #region Appearance

        public string TitleBarBackgroundHexColor {
            get {
                if(IsMainWindowActive) {
                    return MpSystemColors.goldenrod.AdjustAlpha(0.7);
                }
                return MpSystemColors.gainsboro.AdjustAlpha(0.7);
            }
        }

        #endregion

        #endregion

        #region Events
        public event EventHandler OnMainWindowShow;
        public event EventHandler OnMainWindowHidden;
        #endregion

        #region Public Methods        

        private static MpMainWindowViewModel _instance;
        public static MpMainWindowViewModel Instance => _instance ?? (_instance = new MpMainWindowViewModel());


        public MpMainWindowViewModel() : base(null) {
            //MpHelpers.RunOnMainThreadAsync(Init);
        }

        public async Task InitAsync() {
            await Task.Delay(1);
            MpConsole.WriteLine("MainWindow Init");
            PropertyChanged += MpMainWindowViewModel_PropertyChanged;

            //SetupMainWindowRect();
            MainWindowTop = MainWindowTopClosed;

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
                case nameof(IsFilterMenuVisible):
                    var rb = (Application.Current.MainWindow as MpMainWindow).MainWindowResizeBehvior;

                    double deltaY = MpMeasurements.Instance.FilterMenuDefaultHeight;
                    if (IsFilterMenuVisible) {
                        MainWindowTop -= deltaY;
                        //MainWindowHeight += deltaY;
                        rb.ResizeHeight(MainWindowHeight + deltaY);
                    } else {
                        MainWindowTop += deltaY;
                        //MainWindowHeight -= deltaY;
                        rb.ResizeHeight(MainWindowHeight - deltaY);
                    }
                    break;
                case nameof(MainWindowHeight):
                    MpClipTrayViewModel.Instance.OnPropertyChanged(nameof(MpClipTrayViewModel.Instance.ClipTrayHeight));
                    //MpMessenger.SendGlobal(MpMessageType.ResizingContent);
                    break;
                case nameof(IsResizing):
                    if(!IsResizing) {
                        MpMessenger.SendGlobal<MpMessageType>(MpMessageType.ResizingMainWindowComplete);
                    }
                    break;
            }
        }
        private void ReceivedSearchBoxViewModelMessage(MpMessageType msg) {
            switch(msg) {
                case MpMessageType.SearchCriteriaItemsChanged:
                    double yDiff = MpSearchBoxViewModel.Instance.SearchCriteriaListBoxHeight - _lastSearchCriteriaHeight;

                    MainWindowHeight += yDiff;
                    //SetupMainWindowRect(false);
                    MainWindowTop = MainWindowTopOpened;

                    _lastSearchCriteriaHeight = MpSearchBoxViewModel.Instance.SearchCriteriaListBoxHeight;
                    //MpClipTrayViewModel.Instance.OnPropertyChanged(nameof(MpClipTrayViewModel.Instance.ClipTrayHeight));
                    break;
            }
        }


        public void ReceivedResizerBehaviorMessage(MpMessageType msg) {
            switch (msg) {
                case MpMessageType.ResizingContent:
                    //IsResizing = true;
                    //if(IsMainWindowLocked) {
                    //    MainWindowTop = 0;
                    //} else {
                    //    MainWindowTop = MpMeasurements.Instance.WorkAreaBottom - MainWindowHeight;
                    //}

                    MainWindowTop = MpMeasurements.Instance.WorkAreaBottom - MainWindowHeight;
                    break;
                case MpMessageType.ResizeContentCompleted:
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
            MpTempFileManager.Shutdown();            
        }
        #endregion

        #region Commands
        public ICommand IncreaseSizeCommand => new RelayCommand(
             () => {
                 IsResizing = true;
                 var mw = Application.Current.MainWindow as MpMainWindow;
                 mw.MainWindowResizeBehvior.Resize(0, 50);
                 IsResizing = false;
             });

        public ICommand DecreaseSizeCommand => new RelayCommand(
             () => {
                 IsResizing = true;
                 var mw = Application.Current.MainWindow as MpMainWindow;
                 mw.MainWindowResizeBehvior.Resize(0, -50);
                 IsResizing = false;
             });

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
                if (Application.Current.MainWindow is MpMainWindow) {
                    // thats good do nothing
                } else {
                    return;
                }

                IsMainWindowOpening = true;

                //Ss = MpHelpers.CopyScreen();
                //MpHelpers.WriteBitmapSourceToFile(@"C:\Users\tkefauver\Desktop\ss.png", Ss);
                

                MpMessenger.SendGlobal<MpMessageType>(MpMessageType.MainWindowOpening);

                
                var mw = (MpMainWindow)Application.Current.MainWindow;
                while(mw == null) {
                    await Task.Delay(100);
                    mw = (MpMainWindow)Application.Current.MainWindow;
                }
                mw.Show();
                mw.Visibility = Visibility.Visible;
                mw.Activate();
                if (MpNotificationCollectionViewModel.Instance.Notifications.Count == 0) {
                    mw.Topmost = true;
                } else {
                    mw.Topmost = false;
                }                

                if (IsMainWindowInitiallyOpening) {
                    //await MpMainWindowResizeBehavior.Instance.ResizeForInitialLoad();
                    IsMainWindowInitiallyOpening = false;
                }

                //SetupMainWindowRect();
                MainWindowTop = MainWindowTopClosed;

                double tt = MpPreferences.ShowMainWindowAnimationMilliseconds;
                double fps = 30;
                //double dt = (_endMainWindowTop - _startMainWindowTop) / tt / (fps / 1000);
                double dt = (MainWindowTopOpened - MainWindowTopClosed) / tt / (fps / 1000);

                var timer = new DispatcherTimer(DispatcherPriority.Normal);
                timer.Interval = TimeSpan.FromMilliseconds(fps);

                timer.Tick += (s, e32) => {
                    //if (MainWindowTop > _endMainWindowTop) {
                    if (MainWindowTop > MainWindowTopOpened) {
                        MainWindowTop += dt;
                    } else {
                        //MainWindowTop = _endMainWindowTop;
                        MainWindowTop = MainWindowTopOpened;
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
                return (Application.Current.MainWindow != null ||
                   //Application.Current.MainWindow.Visibility != Visibility.Visible ||
                   !IsMainWindowLoading ||
                   !IsShowingDialog) && 
                   !IsMainWindowOpen && !IsMainWindowOpening;
            });

        public ICommand HideWindowCommand => new RelayCommand(
            () => {
                if (IsMainWindowLocked || IsResizing || IsMainWindowClosing || IsShowingDialog) {
                    return;
                }
                var mw = (MpMainWindow)Application.Current.MainWindow;

                if (IsMainWindowOpen) {
                    IsMainWindowClosing = true;

                    double tt = MpPreferences.HideMainWindowAnimationMilliseconds;
                    double fps = 30;
                    //double dt = (_endMainWindowTop - _startMainWindowTop) / tt / (fps / 1000);
                    double dt = (MainWindowTopOpened - MainWindowTopClosed) / tt / (fps / 1000);

                    var timer = new DispatcherTimer(DispatcherPriority.Render);
                    timer.Interval = TimeSpan.FromMilliseconds(fps);

                    timer.Tick += (s, e32) => {
                        //if (MainWindowTop < _startMainWindowTop) {
                        if (MainWindowTop < MainWindowTopClosed) {
                            MainWindowTop -= dt;
                        } else {
                            //MainWindowTop = _startMainWindowTop;
                            MainWindowTop = MainWindowTopClosed;
                            timer.Stop();
                            mw.Visibility = Visibility.Collapsed;

                            IsMainWindowLocked = false;
                            IsMainWindowOpen = false;
                            IsMainWindowClosing = false;

                            OnMainWindowHidden?.Invoke(this, null);
                        }
                    };
                    timer.Start();
                } 
            },
            () => {
                return Application.Current.MainWindow != null &&
                      Application.Current.MainWindow.Visibility == Visibility.Visible &&
                      !IsAnyDropDownOpen &&
                      !IsShowingDialog &&
                      !MpDragDropManager.IsDragAndDrop &&
                      !MpContextMenuView.Instance.IsOpen &&
                      !IsResizing &&
                      !IsMainWindowClosing &&
                      IsMainWindowOpen &&
                      !IsMainWindowOpening;
            });
        
        public ICommand ToggleMainWindowLockCommand => new RelayCommand(
            () => {
                IsMainWindowLocked = !IsMainWindowLocked;
            });

        public ICommand ToggleFilterMenuVisibleCommand => new RelayCommand(
            () => {
                IsFilterMenuVisible = !IsFilterMenuVisible;
            });
        #endregion
    }
}