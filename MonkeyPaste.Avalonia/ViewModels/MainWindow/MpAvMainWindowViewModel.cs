using Avalonia.Controls;
using Avalonia.Threading;
using MonkeyPaste.Common;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MonkeyPaste.Avalonia {
    public enum MpMainWindowOrientationType {
        Left,
        Right,
        Bottom,
        Top
    };

    public enum MpMainWindowShowBehaviorType {
        Primary,
        Mouse,
        All
    };

    public enum MpTaskbarLocation {
        None,
        Bottom,
        Right,
        Top,
        Left
    }

    public class MpAvMainWindowViewModel : MpViewModelBase {
        #region Statics

        private static MpAvMainWindowViewModel _instance;
        public static MpAvMainWindowViewModel Instance = _instance ??= (new MpAvMainWindowViewModel());
        #endregion


        #region Properties

        #region Layout

        public double MainWindowWidth { get; set; }
        public double MainWindowHeight { get; set; }

        public double MainWindowLeft { get; set; }
        public double MainWindowRight { get; set; }
        public double MainWindowTop { get; set; }
        public double MainWindowBottom { get; set; }

        #region Resize Constraints

        public double MainWindowMinHeight {
            get {
                var screen = MpPlatformWrapper.Services.ScreenInfoCollection.Screens.ElementAt(MainWindowMonitorIdx);

                switch (MainWindowOrientationType) {
                    case MpMainWindowOrientationType.Top:
                    case MpMainWindowOrientationType.Bottom:
                        return 100;
                    case MpMainWindowOrientationType.Left:
                    case MpMainWindowOrientationType.Right:
                        return screen.WorkArea.Height;
                }
                return 0;
            }
        }

        public double MainWindowMaxHeight {
            get {
                var screen = MpPlatformWrapper.Services.ScreenInfoCollection.Screens.ElementAt(MainWindowMonitorIdx);
                double pad = 20;

                switch (MainWindowOrientationType) {
                    case MpMainWindowOrientationType.Top:
                    case MpMainWindowOrientationType.Bottom:
                        return screen.WorkArea.Height - pad;
                    case MpMainWindowOrientationType.Left:
                    case MpMainWindowOrientationType.Right:
                        return screen.WorkArea.Height;
                }
                return 0;
            }
        }

        public double MainWindowMinWidth {
            get {
                var screen = MpPlatformWrapper.Services.ScreenInfoCollection.Screens.ElementAt(MainWindowMonitorIdx);

                switch (MainWindowOrientationType) {
                    case MpMainWindowOrientationType.Top:
                    case MpMainWindowOrientationType.Bottom:
                        return screen.WorkArea.Width;
                    case MpMainWindowOrientationType.Left:
                    case MpMainWindowOrientationType.Right:
                        return 100;
                }
                return 0;
            }
        }

        public double MainWindowMaxWidth {
            get {
                var screen = MpPlatformWrapper.Services.ScreenInfoCollection.Screens.ElementAt(MainWindowMonitorIdx);
                double pad = 20;

                switch (MainWindowOrientationType) {
                    case MpMainWindowOrientationType.Top:
                    case MpMainWindowOrientationType.Bottom:
                        return screen.WorkArea.Width;
                    case MpMainWindowOrientationType.Left:
                    case MpMainWindowOrientationType.Right:
                        return screen.WorkArea.Width - pad;
                }
                return 0;
            }
        }

        public double MainWindowDefaultWidth {
            get {
                var screen = MpPlatformWrapper.Services.ScreenInfoCollection.Screens.ElementAt(MainWindowMonitorIdx);

                switch (MainWindowOrientationType) {
                    case MpMainWindowOrientationType.Top:
                    case MpMainWindowOrientationType.Bottom:
                        return screen.WorkArea.Width;
                    case MpMainWindowOrientationType.Left:
                    case MpMainWindowOrientationType.Right:
                        return screen.WorkArea.Width * 0.3d;
                }
                return 0;
            }
        }

        public double MainWindowDefaultHeight {
            get {
                var screen = MpPlatformWrapper.Services.ScreenInfoCollection.Screens.ElementAt(MainWindowMonitorIdx);
               
                switch (MainWindowOrientationType) {
                    case MpMainWindowOrientationType.Top:
                    case MpMainWindowOrientationType.Bottom:
                        return screen.WorkArea.Height * 0.35d;
                    case MpMainWindowOrientationType.Left:
                    case MpMainWindowOrientationType.Right:
                        return screen.WorkArea.Height;
                }
                return 0;
            }
        }

        #endregion

        public MpRect MainWindowRect => new MpRect(MainWindowLeft, MainWindowTop, MainWindowRight, MainWindowBottom);
        
        public MpRect ExternalRect {
            get {
                var screen = MpPlatformWrapper.Services.ScreenInfoCollection.Screens.ElementAt(MainWindowMonitorIdx);

                switch (MainWindowOrientationType) {
                    case MpMainWindowOrientationType.Top:
                        double y = MainWindowTop + MainWindowHeight;
                        if(y < 0) {
                            y = 0;
                        }
                        double h = y == 0 ? screen.Bounds.Height : screen.Bounds.Height - (MainWindowTop + MainWindowHeight);

                        var extRect = new MpRect(0, y, MainWindowWidth, h);
                        MpConsole.WriteLine("Ext Rect: " + extRect);
                        return extRect;
                }
                return new MpRect();
            }
        }
        public double ExternalTop => ExternalRect.Top;

        public double ExternalHeight => ExternalRect.Height;
        public MpRect MainWindowOpenedRect {
            get {
                var screen = MpPlatformWrapper.Services.ScreenInfoCollection.Screens.ElementAt(MainWindowMonitorIdx);
                if (screen == null) {
                    Debugger.Break();
                    return new MpRect();
                }

                switch (MainWindowOrientationType) {
                    case MpMainWindowOrientationType.Bottom:
                        return new MpRect(
                            screen.WorkArea.Left,
                            screen.WorkArea.Bottom - MainWindowHeight,
                            MainWindowWidth,
                            MainWindowHeight);
                    case MpMainWindowOrientationType.Top:
                        return new MpRect(
                            screen.WorkArea.Left,
                            screen.WorkArea.Top,
                            MainWindowWidth,
                            MainWindowHeight);
                    case MpMainWindowOrientationType.Left:
                        return new MpRect(
                            screen.WorkArea.Left,
                            screen.WorkArea.Top,
                            MainWindowWidth,
                            MainWindowHeight);
                    case MpMainWindowOrientationType.Right:
                        return new MpRect(
                            screen.WorkArea.Right - MainWindowWidth,
                            screen.WorkArea.Top,
                            MainWindowWidth,
                            MainWindowHeight);
                }

                return new MpRect();
            }
        }

        public MpRect MainWindowClosedRect {
            get {
                var screen = MpPlatformWrapper.Services.ScreenInfoCollection.Screens.ElementAt(MainWindowMonitorIdx);
                if (screen == null) {
                    Debugger.Break();
                    return new MpRect();
                }

                switch (MainWindowOrientationType) {
                    case MpMainWindowOrientationType.Bottom:
                        return new MpRect(
                            screen.WorkArea.Left,
                            screen.WorkArea.Bottom,
                            MainWindowWidth,
                            MainWindowHeight);
                    case MpMainWindowOrientationType.Top:
                        return new MpRect(
                            screen.WorkArea.Left,
                            screen.WorkArea.Top - MainWindowHeight,
                            MainWindowWidth,
                            MainWindowHeight);
                    case MpMainWindowOrientationType.Left:
                        return new MpRect(
                            screen.WorkArea.Left - MainWindowWidth,
                            screen.WorkArea.Top,
                            MainWindowWidth,
                            MainWindowHeight);
                    case MpMainWindowOrientationType.Right:
                        return new MpRect(
                            screen.WorkArea.Right,
                            screen.WorkArea.Top,
                            MainWindowWidth,
                            MainWindowHeight);
                }

                return new MpRect();
            }
        }

        #endregion

        #region Appearance

        public string TitleBarBackgroundHexColor {
            get {
                if (IsMainWindowActive) {
                    return MpSystemColors.goldenrod.AdjustAlpha(0.7);
                }
                return MpSystemColors.gainsboro.AdjustAlpha(0.7);
            }
        }

        #endregion

        #region State
        
        public bool IsHovering { get; set; }

        public bool IsMainWindowInitiallyOpening { get; set; } = true;

        public bool IsMainWindowOpening { get; set; } = false;

        public bool IsMainWindowClosing { get; set; } = false;

        public bool IsMainWindowOpen { get; private set; } = false;

        public bool IsMainWindowLoading { get; set; } = true;

        public bool IsMainWindowLocked { get; set; } = false;

        public bool IsResizing { get; set; } = false;

        public bool CanResize { get; set; } = false;

        public bool IsAnyTextBoxFocused { get; set; }

        public bool IsAnyDropDownOpen { get; set; }

        public bool IsShowingDialog { get; set; } = false;

        public bool IsMainWindowActive { get; set; }

        public bool IsFilterMenuVisible { get; set; } = true;

        public MpResizeEdgeType ResizerEdge {
            get {
                if(MainWindowOrientationType == MpMainWindowOrientationType.Left ||
                   MainWindowOrientationType == MpMainWindowOrientationType.Right) {
                    return MpResizeEdgeType.Right;
                }
                return MpResizeEdgeType.Top;
            }
        }

        public MpMainWindowOrientationType MainWindowOrientationType {
            get => (MpMainWindowOrientationType)Enum.Parse(typeof(MpMainWindowOrientationType), MpPrefViewModel.Instance.MainWindowOrientation, false);
            set => MpPrefViewModel.Instance.MainWindowOrientation = value.ToString();
        }

        public MpMainWindowShowBehaviorType MainWindowShowBehaviorType {
            get => (MpMainWindowShowBehaviorType)Enum.Parse(typeof(MpMainWindowShowBehaviorType), MpPrefViewModel.Instance.MainWindowDisplayType, false);
            set => MpPrefViewModel.Instance.MainWindowDisplayType = value.ToString();
        }

        public int MainWindowMonitorIdx {
            get {
                switch(MainWindowShowBehaviorType) {
                    case MpMainWindowShowBehaviorType.Primary:
                    default:
                        // NOTE will need another monitor to build out non-primary display types
                        return MpPlatformWrapper.Services.ScreenInfoCollection.Screens.IndexOf(x => x.IsPrimary);
                }
            }
        }


        #endregion

        #endregion

        #region Events

        public event EventHandler? OnMainWindowOpened;

        public event EventHandler? OnMainWindowClosed;
        #endregion

        public MpAvMainWindowViewModel() : base() {
            PropertyChanged += MpAvMainWindowViewModel_PropertyChanged;
        }       


        #region Public Methods

        public async Task InitializeAsync() {
            await Task.Delay(1);

            SetupMainWindowSize();

            IsMainWindowLoading = false;
            
            ShowWindowCommand.Execute(null);

        }

        public void SetupMainWindowSize(bool isOrientationChange = false) {
            var screen = MpPlatformWrapper.Services.ScreenInfoCollection.Screens.ElementAt(MainWindowMonitorIdx);
            if(screen == null) {
                Debugger.Break();
                return;
            }
            switch (MainWindowOrientationType) {
                case MpMainWindowOrientationType.Top:
                case MpMainWindowOrientationType.Bottom:
                    MainWindowWidth = screen.WorkArea.Width;
                    if(MainWindowHeight == 0) {
                        // startup case                        
                        if (MpPrefViewModel.Instance.MainWindowInitialHeight == 0) {
                            // initial setting
                            MpPrefViewModel.Instance.MainWindowInitialHeight = screen.WorkArea.Height * 0.35;
                        }
                        MainWindowHeight = MpPrefViewModel.Instance.MainWindowInitialHeight;                        
                    } else {
                        if (isOrientationChange) {
                            // clear initial width 
                            MpPrefViewModel.Instance.MainWindowInitialWidth = 0;
                            // reset height and call again to propagate initial height setting
                            MainWindowHeight = 0;
                            SetupMainWindowSize(false);
                        } else {
                            // height is user defined
                        }
                    }

                    break;
                case MpMainWindowOrientationType.Left:
                case MpMainWindowOrientationType.Right:
                    MainWindowHeight = screen.WorkArea.Height;
                    if (MainWindowWidth == 0) {
                        // startup case                        
                        if (MpPrefViewModel.Instance.MainWindowInitialWidth == 0) {
                            // initial setting
                            MpPrefViewModel.Instance.MainWindowInitialWidth = screen.WorkArea.Width * 0.3;
                        }
                        MainWindowWidth = MpPrefViewModel.Instance.MainWindowInitialWidth;
                    } else {
                        if (isOrientationChange) {
                            // clear initial height
                            MpPrefViewModel.Instance.MainWindowInitialHeight = 0;
                            // reset Width and call again to propagate initial width setting
                            MainWindowWidth = 0;
                            SetupMainWindowSize(false);
                        } else {
                            // width is user defined
                        }
                    }

                    break;
            }
            
        }

        #endregion

        #region Private Methods

        private void MpAvMainWindowViewModel_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e) {
            switch(e.PropertyName) {
                case nameof(IsHovering):
                    MpConsole.WriteLine("MainWindow Hover: " + (IsHovering ? "TRUE":"FALSE"));
                    break;
                case nameof(MainWindowHeight):
                    if(!IsResizing) {
                        return;
                    }
                    MainWindowTop = MainWindowOpenedRect.Top;
                    MainWindowBottom = MainWindowOpenedRect.Bottom;
                    break;
                case nameof(MainWindowWidth):
                    if (!IsResizing) {
                        return;
                    }
                    MainWindowLeft = MainWindowOpenedRect.Left;
                    MainWindowRight = MainWindowOpenedRect.Right;
                    break;
                case nameof(IsResizing):
                    if(!IsResizing) {
                        // after resizing store new resized dimension for next load                        

                        if (MainWindowOrientationType == MpMainWindowOrientationType.Left ||
                            MainWindowOrientationType == MpMainWindowOrientationType.Right) {
                            MpPrefViewModel.Instance.MainWindowInitialWidth = MainWindowWidth;
                        } else {
                            MpPrefViewModel.Instance.MainWindowInitialHeight = MainWindowHeight;
                        }

                        // update location (needed if reset to default)

                        //MainWindowTop = MainWindowOpenedRect.Top;
                        //MainWindowBottom = MainWindowOpenedRect.Bottom;
                        //MainWindowLeft = MainWindowOpenedRect.Left;
                        //MainWindowRight = MainWindowOpenedRect.Right;
                    }
                    break;
            }
        }
        #endregion


        #region Commands        

        public ICommand ShowWindowCommand => new MpAsyncCommand(
            async () => {
                if(!Dispatcher.UIThread.CheckAccess()) {
                    Dispatcher.UIThread.Post(() => {
                        ShowWindowCommand.Execute(null);
                    });
                    return;
                }
                IsMainWindowOpening = true;

                //MpMessenger.SendGlobal<MpMessageType>(MpMessageType.MainWindowOpening);

                var mw = MainWindow.Instance;
                while (mw == null) {
                    await Task.Delay(100);
                    mw = MainWindow.Instance;
                }
                mw.Show();
                mw.IsVisible = true;
                mw.WindowState = WindowState.Maximized;
                mw.Activate();
                mw.Topmost = false;

                if (IsMainWindowInitiallyOpening) {
                    //await MpMainWindowResizeBehavior.Instance.ResizeForInitialLoad();
                    IsMainWindowInitiallyOpening = false;
                }

                MpRect openStartRect = MainWindowClosedRect;
                MainWindowLeft = openStartRect.Left;
                MainWindowTop = openStartRect.Top;
                MainWindowRight = openStartRect.Right;
                MainWindowBottom = openStartRect.Bottom;

                //MpConsole.WriteLine($"SHOW WINDOW START: L: " + MainWindowLeft + " T: " + MainWindowTop + " R:" + MainWindowRight + " B:" + MainWindowBottom);

                MpRect openEndRect = MainWindowOpenedRect;

                double test = MpPrefViewModel.Instance.ShowMainWindowAnimationMilliseconds;
                double tt = 500;
                double fps = 30;
                double step = tt / (fps );

                double d_l = (openEndRect.Left - openStartRect.Left) / step;
                double d_t = (openEndRect.Top - openStartRect.Top) / step;
                double d_r = (openEndRect.Right - openStartRect.Right) / step;
                double d_b = (openEndRect.Bottom - openStartRect.Bottom) / step;
                
                //MpConsole.WriteLine($"SHOW WINDOW STEP: L: " + d_l + " T: " + d_t + " R:" + d_r + " B:" + d_b);

                var timer = new DispatcherTimer(DispatcherPriority.Normal);
                timer.Interval = TimeSpan.FromMilliseconds(fps);

                timer.Tick += (s, e32) => {
                    bool isDone = false;
                    switch(MainWindowOrientationType) {
                        case MpMainWindowOrientationType.Bottom:
                            isDone = MainWindowTop < openEndRect.Top;
                            break;
                        case MpMainWindowOrientationType.Top:
                            isDone = MainWindowTop > openEndRect.Top;
                            break;
                        case MpMainWindowOrientationType.Left:
                            isDone = MainWindowLeft > openEndRect.Left;
                            break;
                        case MpMainWindowOrientationType.Right:
                            isDone = MainWindowLeft < openEndRect.Left;
                            break;
                    }
                    if(isDone) {
                        //MpConsole.WriteLine("SHOW WINDOW DONE");
                        MainWindowLeft = openEndRect.Left;
                        MainWindowTop = openEndRect.Top;
                        MainWindowRight = openEndRect.Right;
                        MainWindowBottom = openEndRect.Bottom;

                        timer.Stop();

                        IsMainWindowLoading = false;
                        IsMainWindowOpening = false;
                        IsMainWindowOpen = true;

                        OnPropertyChanged(nameof(ExternalRect));
                        OnMainWindowOpened?.Invoke(this, new EventArgs());

                        //MpClipTrayViewModel.Instance.AddNewItemsCommand.Execute(null);
                    } else {
                        //MpConsole.WriteLine($"SHOW WINDOW ANIMATING: L: " + MainWindowLeft + " T: " + MainWindowTop + " R:" + MainWindowRight + " B:" + MainWindowBottom);
                        MainWindowLeft += d_l;
                        MainWindowTop += d_t;
                        MainWindowRight += d_r;
                        MainWindowBottom += d_b;

                        OnPropertyChanged(nameof(ExternalRect));
                    }
                };

                timer.Start();
            },
            () => {
                return (MainWindow.Instance != null ||
                   !IsMainWindowLoading ||
                   !IsShowingDialog) &&
                   !IsMainWindowOpen && !IsMainWindowOpening;
            });

        public ICommand HideWindowCommand => new MpCommand(
            () => {
                if (IsMainWindowLocked || IsResizing || IsMainWindowClosing || IsShowingDialog) {
                    return;
                }
                var mw = MainWindow.Instance;

                if (IsMainWindowOpen) {
                    IsMainWindowClosing = true;

                    MpRect closeStartRect = MainWindowOpenedRect;
                    MainWindowLeft = closeStartRect.Left;
                    MainWindowTop = closeStartRect.Top;
                    MainWindowRight = closeStartRect.Right;
                    MainWindowBottom = closeStartRect.Bottom;

                    MpRect closeEndRect = MainWindowClosedRect;

                    double tt = MpPrefViewModel.Instance.HideMainWindowAnimationMilliseconds;
                    double fps = 30;
                    double step = tt / (fps);

                    double d_l = (closeEndRect.Left - closeStartRect.Left) / step;
                    double d_t = (closeEndRect.Top - closeStartRect.Top) / step;
                    double d_r = (closeEndRect.Right - closeStartRect.Right) / step;
                    double d_b = (closeEndRect.Bottom - closeStartRect.Bottom) / step;

                    var timer = new DispatcherTimer(DispatcherPriority.Normal);
                    timer.Interval = TimeSpan.FromMilliseconds(fps);

                    timer.Tick += (s, e32) => {
                        bool isDone = false;
                        switch (MainWindowOrientationType) {
                            case MpMainWindowOrientationType.Bottom:
                                isDone = MainWindowTop > closeEndRect.Top;
                                break;
                            case MpMainWindowOrientationType.Top:
                                isDone = MainWindowTop < closeEndRect.Top;
                                break;
                            case MpMainWindowOrientationType.Left:
                                isDone = MainWindowLeft < closeEndRect.Left;
                                break;
                            case MpMainWindowOrientationType.Right:
                                isDone = MainWindowLeft > closeEndRect.Left;
                                break;
                        }
                        if (isDone) {
                            MainWindowLeft = closeEndRect.Left;
                            MainWindowTop = closeEndRect.Top;
                            MainWindowRight = closeEndRect.Right;
                            MainWindowBottom = closeEndRect.Bottom;

                            timer.Stop();
                            mw!.IsVisible = false;
                            mw!.WindowState = WindowState.Minimized;
                            
                            //MpConsole.WriteLine("HIDE WINDOW DONE");

                            IsMainWindowLoading = false;
                            IsMainWindowOpening = false;
                            IsMainWindowOpen = true;

                            IsMainWindowLocked = false;
                            IsMainWindowOpen = false;
                            IsMainWindowClosing = false;

                            OnPropertyChanged(nameof(ExternalRect));

                            OnMainWindowClosed?.Invoke(this, new EventArgs());
                        } else {
                            MainWindowLeft += d_l;
                            MainWindowTop += d_t;
                            MainWindowRight += d_r;
                            MainWindowBottom += d_b;

                            OnPropertyChanged(nameof(ExternalRect));
                        }
                    };
                    timer.Start();
                }
            },
            () => {
                return MainWindow.Instance != null &&
                      MainWindow.Instance.IsVisible &&
                      !IsAnyDropDownOpen &&
                      !IsShowingDialog &&
                      //!MpDragDropManager.IsDragAndDrop &&
                      //!MpContextMenuView.Instance.IsOpen &&
                      !IsResizing &&
                      !IsMainWindowClosing &&
                      IsMainWindowOpen &&
                      !IsMainWindowOpening;
            });



        //public ICommand ToggleMainWindowLockCommand => new RelayCommand(
        //    () => {
        //        IsMainWindowLocked = !IsMainWindowLocked;
        //    });

        //public ICommand ToggleFilterMenuVisibleCommand => new RelayCommand(
        //    () => {
        //        IsFilterMenuVisible = !IsFilterMenuVisible;
        //    });

        //public ICommand IncreaseSizeCommand => new RelayCommand(
        //     () => {
        //         IsResizing = true;
        //         var mw = Application.Current.MainWindow as MpMainWindow;
        //         mw.MainWindowResizeBehvior.Resize(0, 50);
        //         IsResizing = false;
        //     });

        //public ICommand DecreaseSizeCommand => new RelayCommand(
        //     () => {
        //         IsResizing = true;
        //         var mw = Application.Current.MainWindow as MpMainWindow;
        //         mw.MainWindowResizeBehvior.Resize(0, -50);
        //         IsResizing = false;
        //     });

        //private RelayCommand _undoCommand;
        //public ICommand UndoCommand {
        //    get {
        //        if (_undoCommand == null) {
        //            _undoCommand = new RelayCommand(() => UndoManager.Undo(), () => UndoManager.CanUndo);
        //        }
        //        return _undoCommand;
        //    }
        //}

        //private ICommand _redoCommand;
        //public ICommand RedoCommand {
        //    get {
        //        if (_redoCommand == null)
        //            _redoCommand = new RelayCommand(() => UndoManager.Redo(), () => UndoManager.CanRedo);
        //        return _redoCommand;
        //    }
        //}
        #endregion



    }
}
