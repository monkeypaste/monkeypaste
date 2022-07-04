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

        public MpRect MainWindowRect => new MpRect(MainWindowLeft, MainWindowTop, MainWindowWidth, MainWindowHeight);
        
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
                }

                return new MpRect();
            }
        }

        #endregion

        #region State
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

        public MpMainWindowOrientationType MainWindowOrientationType {
            get => (MpMainWindowOrientationType)Enum.Parse(typeof(MpMainWindowOrientationType), MpJsonPreferenceIO.Instance.MainWindowOrientation, false);
            set => MpJsonPreferenceIO.Instance.MainWindowOrientation = value.ToString();
        }

        public MpMainWindowShowBehaviorType MainWindowShowBehaviorType {
            get => (MpMainWindowShowBehaviorType)Enum.Parse(typeof(MpMainWindowShowBehaviorType), MpJsonPreferenceIO.Instance.MainWindowDisplayType, false);
            set => MpJsonPreferenceIO.Instance.MainWindowDisplayType = value.ToString();
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



        #region Public Methods

        public async Task InitializeAsync() {
            await Task.Delay(1);

            SetupMainWindowSize();

            //MainWindowLeft = MainWindowClosedRect.Left;
            //MainWindowTop = MainWindowClosedRect.Top;
            //MainWindowRight = MainWindowClosedRect.Right;
            //MainWindowBottom = MainWindowClosedRect.Bottom;

            IsMainWindowLoading = false;
            
            ShowWindowCommand.Execute(null);

        }

        public void SetupMainWindowSize() {
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
                        if (MpJsonPreferenceIO.Instance.MainWindowInitialHeight == 0) {
                            // initial setting
                            MpJsonPreferenceIO.Instance.MainWindowInitialHeight = screen.WorkArea.Height * 0.35;
                        }
                        MainWindowHeight = MpJsonPreferenceIO.Instance.MainWindowInitialHeight;                        
                    } else {
                        // height is user defined
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
                mw!.Show();
                mw.IsVisible = true;
                MainWindow.Instance.WindowState = WindowState.Maximized;
                mw.Activate();
                if (MpNotificationCollectionViewModel.Instance.Notifications.Count == 0) {
                    mw.Topmost = true;
                } else {
                    mw.Topmost = false;
                }
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

                MpConsole.WriteLine($"SHOW WINDOW START: L: " + MainWindowLeft + " T: " + MainWindowTop + " R:" + MainWindowRight + " B:" + MainWindowBottom);

                MpRect openEndRect = MainWindowOpenedRect;

                double test = MpJsonPreferenceIO.Instance.ShowMainWindowAnimationMilliseconds;
                double tt = 500;//
                double fps = 30;

                //double dt = (MainWindowTopOpened - MainWindowTopClosed) / tt / (fps / 1000);
                double step = tt / (fps );

                double d_l = (openEndRect.Left - openStartRect.Left) / step;
                double d_t = (openEndRect.Top - openStartRect.Top) / step;
                double d_r = (openEndRect.Right - openStartRect.Right) / step;
                double d_b = (openEndRect.Bottom - openStartRect.Bottom) / step;
                
                MpConsole.WriteLine($"SHOW WINDOW STEP: L: " + d_l + " T: " + d_t + " R:" + d_r + " B:" + d_b);

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
                    }
                    if(isDone) {
                        MpConsole.WriteLine("SHOW WINDOW DONE");
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
                        MpConsole.WriteLine($"SHOW WINDOW ANIMATING: L: " + MainWindowLeft + " T: " + MainWindowTop + " R:" + MainWindowRight + " B:" + MainWindowBottom);
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

                    double tt = MpJsonPreferenceIO.Instance.HideMainWindowAnimationMilliseconds;
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
                        }
                        if (isDone) {
                            MainWindowLeft = closeEndRect.Left;
                            MainWindowTop = closeEndRect.Top;
                            MainWindowRight = closeEndRect.Right;
                            MainWindowBottom = closeEndRect.Bottom;

                            timer.Stop();
                            MainWindow.Instance.IsVisible = false;
                            MainWindow.Instance.WindowState = WindowState.Minimized;
                            
                            MpConsole.WriteLine("HIDE WINDOW DONE");

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
