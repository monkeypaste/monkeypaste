using Avalonia;
using Avalonia.Controls;
using Avalonia.Threading;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Avalonia;
using MonkeyPaste.Common.Utils.Extensions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MonkeyPaste.Avalonia {
    public enum MpMainWindowOrientationType {
        Bottom,
        Right,
        Top,
        Left
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
    public class MpAvMainWindowViewModel : MpViewModelBase, MpIResizableViewModel {
        #region Private Variables

        private double _resize_shortcut_nudge_amount = 50;

        #endregion

        #region Statics

        private static MpAvMainWindowViewModel _instance;

        public static MpAvMainWindowViewModel Instance => _instance ?? (_instance = new MpAvMainWindowViewModel());
        #endregion

        #region Properties

        #region View Models

        public MpIOrientedSidebarItemViewModel SelectedSidebarItemViewModel {
            get {
                if (MpAvTagTrayViewModel.Instance.IsSidebarVisible) {
                    return MpAvTagTrayViewModel.Instance;
                }
                if (MpActionCollectionViewModel.Instance.IsSidebarVisible) {
                    return MpActionCollectionViewModel.Instance;
                }
                if (MpAnalyticItemCollectionViewModel.Instance.IsSidebarVisible) {
                    return MpAnalyticItemCollectionViewModel.Instance;
                }
                return null;
            }
        }
        #endregion

        #region Layout

        public double MainWindowDefaultHorizontalHeightRatio => 0.35;
        public double MainWindowDefaultVerticalWidthRatio => 0.2;

        public double MainWindowWidth { get; set; }
        public double MainWindowHeight { get; set; }

        public double MainWindowLeft { get; set; }

        public double MainWindowRight { get; set; }

        public double MainWindowTop { get; set; }

        public double MainWindowBottom { get; set; }


        #region Resize Constraints

        public double ResizerLength => 3;

        public double MainWindowMinimumHorizontalHeight => 200;
        public double MainWindowMinimumVerticalWidth => 290;
        public double MainWindowExtentPad => 20;

        public double ResizeXFactor {
            get {
                switch (MainWindowOrientationType) {
                    case MpMainWindowOrientationType.Top:
                    case MpMainWindowOrientationType.Bottom:
                        return 0d;
                    case MpMainWindowOrientationType.Left:
                        return -1.0d;
                    case MpMainWindowOrientationType.Right:
                        return 1.0d;
                }
                return 0;
            }
        }
        public double ResizeYFactor {
            get {
                switch (MainWindowOrientationType) {
                    case MpMainWindowOrientationType.Top:
                        return -1d;
                    case MpMainWindowOrientationType.Bottom:
                        return 1d;
                    case MpMainWindowOrientationType.Left:
                    case MpMainWindowOrientationType.Right:
                        return 0d;
                }
                return 0;
            }
        }

        public double MainWindowMinHeight {
            get {
                switch (MainWindowOrientationType) {
                    case MpMainWindowOrientationType.Top:
                    case MpMainWindowOrientationType.Bottom:
                        return MainWindowMinimumHorizontalHeight;
                    case MpMainWindowOrientationType.Left:
                    case MpMainWindowOrientationType.Right:
                        return MainWindowScreen.WorkArea.Height;
                }
                return 0;
            }
        }

        public double MainWindowMaxHeight {
            get {
                switch (MainWindowOrientationType) {
                    case MpMainWindowOrientationType.Top:
                    case MpMainWindowOrientationType.Bottom:
                        return MainWindowScreen.WorkArea.Height - MainWindowExtentPad;
                    case MpMainWindowOrientationType.Left:
                    case MpMainWindowOrientationType.Right:
                        return MainWindowScreen.WorkArea.Height;
                }
                return 0;
            }
        }

        public double MainWindowMinWidth {
            get {
                switch (MainWindowOrientationType) {
                    case MpMainWindowOrientationType.Top:
                    case MpMainWindowOrientationType.Bottom:
                        return MainWindowScreen.WorkArea.Width;
                    case MpMainWindowOrientationType.Left:
                    case MpMainWindowOrientationType.Right:
                        return MainWindowMinimumVerticalWidth;
                }
                return 0;
            }
        }

        public double MainWindowMaxWidth {
            get {
                switch (MainWindowOrientationType) {
                    case MpMainWindowOrientationType.Top:
                    case MpMainWindowOrientationType.Bottom:
                        return MainWindowScreen.WorkArea.Width;
                    case MpMainWindowOrientationType.Left:
                    case MpMainWindowOrientationType.Right:
                        return MainWindowScreen.WorkArea.Width - MainWindowExtentPad;
                }
                return 0;
            }
        }

        public double MainWindowDefaultWidth {
            get {

                switch (MainWindowOrientationType) {
                    case MpMainWindowOrientationType.Top:
                    case MpMainWindowOrientationType.Bottom:
                        return MainWindowScreen.WorkArea.Width;
                    case MpMainWindowOrientationType.Left:
                    case MpMainWindowOrientationType.Right:
                        return MainWindowScreen.WorkArea.Width * MainWindowDefaultVerticalWidthRatio;
                }
                return 0;
            }
        }

        public double MainWindowDefaultHeight {
            get {
                switch (MainWindowOrientationType) {
                    case MpMainWindowOrientationType.Top:
                    case MpMainWindowOrientationType.Bottom:
                        return MainWindowScreen.WorkArea.Height * MainWindowDefaultHorizontalHeightRatio;
                    case MpMainWindowOrientationType.Left:
                    case MpMainWindowOrientationType.Right:
                        return MainWindowScreen.WorkArea.Height;
                }
                return 0;
            }
        }

        #endregion

        // Last and Cur Rect set in view bounds changed handler
        public MpRect LastMainWindowRect { get; set; } = new MpRect();
        public MpRect MainWindowRect { get; set; } = new MpRect();

        public MpRect MainWindowScreenRect => new MpRect(MainWindowLeft, MainWindowTop, MainWindowRight - MainWindowLeft, MainWindowBottom - MainWindowTop); //{
        //    get {
        //        if(MpAvMainWindow.Instance == null) {
        //            return MpRect.Empty;
        //        }
        //        var pd = MainWindowScreen.PixelDensity;
        //        var mw_screen_origin = new MpPoint((double)MpAvMainWindow.Instance.Position.X / pd, (double)MpAvMainWindow.Instance.Position.Y / pd);
        //        return new MpRect(mw_screen_origin, MainWindowRect.Size);
        //    }
        //}

        public MpRect ExternalRect {
            get {
                switch (MainWindowOrientationType) {
                    case MpMainWindowOrientationType.Top:
                        double y = MainWindowTop + MainWindowHeight;
                        if (y < 0) {
                            y = 0;
                        }
                        double h = y == 0 ? MainWindowScreen.Bounds.Height :
                                    MainWindowScreen.Bounds.Height - (MainWindowTop + MainWindowHeight);

                        var extRect = new MpRect(0, y, MainWindowWidth, h);
                        MpConsole.WriteLine("Ext Rect: " + extRect);
                        return extRect;
                }
                return new MpRect();
            }
        }

        public double ExternalTop => ExternalRect.Top;

        public double ExternalHeight => ExternalRect.Height;
        public MpRect MainWindowOpenedScreenRect {
            get {
                switch (MainWindowOrientationType) {
                    case MpMainWindowOrientationType.Bottom:
                        return new MpRect(
                            MainWindowScreen.WorkArea.Left,
                            MainWindowScreen.WorkArea.Bottom - MainWindowHeight,
                            MainWindowWidth,
                            MainWindowHeight);
                    case MpMainWindowOrientationType.Top:
                        return new MpRect(
                            MainWindowScreen.WorkArea.Left,
                            MainWindowScreen.WorkArea.Top,
                            MainWindowWidth,
                            MainWindowHeight);
                    case MpMainWindowOrientationType.Left:
                        return new MpRect(
                            MainWindowScreen.WorkArea.Left,
                            MainWindowScreen.WorkArea.Top,
                            MainWindowWidth,
                            MainWindowHeight);
                    case MpMainWindowOrientationType.Right:
                        return new MpRect(
                            MainWindowScreen.WorkArea.Right - MainWindowWidth,
                            MainWindowScreen.WorkArea.Top,
                            MainWindowWidth,
                            MainWindowHeight);
                }

                return new MpRect();
            }
        }

        public MpRect MainWindowClosedScreenRect {
            get {
                switch (MainWindowOrientationType) {
                    case MpMainWindowOrientationType.Bottom:
                        return new MpRect(
                            MainWindowScreen.WorkArea.Left,
                            MainWindowScreen.WorkArea.Bottom,
                            MainWindowWidth,
                            MainWindowHeight);
                    case MpMainWindowOrientationType.Top:
                        return new MpRect(
                            MainWindowScreen.WorkArea.Left,
                            MainWindowScreen.WorkArea.Top - MainWindowHeight,
                            MainWindowWidth,
                            MainWindowHeight);
                    case MpMainWindowOrientationType.Left:
                        return new MpRect(
                            MainWindowScreen.WorkArea.Left - MainWindowWidth,
                            MainWindowScreen.WorkArea.Top,
                            MainWindowWidth,
                            MainWindowHeight);
                    case MpMainWindowOrientationType.Right:
                        return new MpRect(
                            MainWindowScreen.WorkArea.Right,
                            MainWindowScreen.WorkArea.Top,
                            MainWindowWidth,
                            MainWindowHeight);
                }

                return new MpRect();
            }
        }

        #endregion

        #region Appearance

        public MpCursorType ResizerCursor {
            get {
                if (IsHorizontalOrientation) {
                    return MpCursorType.ResizeNS;
                } else {
                    return MpCursorType.ResizeWE;
                }
            }
        }

        #endregion

        #region State

        public MpPoint DragMouseMainWindowLocation { get; set; }
        public bool IsDropOverMainWindow { get; private set; } = false;
        public bool IsResizerVisible { get; set; } = false;     
        public bool IsHovering { get; set; }

        public bool IsMainWindowInitiallyOpening { get; set; } = true;

        public bool IsMainWindowOpening { get; private set; }
        //    get {
        //        if(IsMainWindowOpen) {
        //            return false;
        //        }
        //        return MainWindowScreenRect.IsAnyPointWithinOtherRect(MainWindowScreen.Bounds);
        //    }
        //}

        public bool IsMainWindowClosing { get; private set; }
        //    get {
        //        if (IsMainWindowOpen) {
        //            return false;
        //        }
        //        if (MainWindowRect.FuzzyEquals(MainWindowClosedRect)) {
        //            return false;
        //        }
        //        return MainWindowScreenRect.IsAnyPointWithinOtherRect(MainWindowScreen.Bounds);
        //    }
        //}

        public bool IsMainWindowOpen { get; private set; } = false;
        //    get {
        //        if(!IsMainWindowVisible) {
        //            return false;
        //        }
        //        return MainWindowScreenRect.FuzzyEquals(MainWindowOpenedScreenRect);
        //    }
        //}

        public bool IsMainWindowVisible { get; set; }

        public bool IsMainWindowLoading { get; set; } = true;

        public bool IsMainWindowLocked { get; set; } = false;

        public bool IsResizing { get; set; } = false;

        public bool CanResize { get; set; } = false;

        public bool IsAnyTextBoxFocused { get; set; }

        public bool IsAnyDropDownOpen { get; set; }

        public bool IsShowingDialog { get; set; } = false;

        public bool IsMainWindowActive { get; set; }

        public bool IsFilterMenuVisible { get; set; } = true;

        public bool IsHorizontalOrientation => MainWindowOrientationType == MpMainWindowOrientationType.Bottom ||
                                                MainWindowOrientationType == MpMainWindowOrientationType.Top;

        public bool IsVerticalOrientation => !IsHorizontalOrientation;
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
                switch (MainWindowShowBehaviorType) {
                    case MpMainWindowShowBehaviorType.Primary:
                    default:
                        // NOTE will need another monitor to build out non-primary display types
                        int monitorIdx = MpPlatformWrapper.Services.ScreenInfoCollection.Screens.IndexOf(x => x.IsPrimary);
                        _mainWindowScreen = monitorIdx < 0 ? null : MpPlatformWrapper.Services.ScreenInfoCollection.Screens.ElementAt(monitorIdx);
                        return monitorIdx;
                }
            }
        }

        private MpIPlatformScreenInfo _mainWindowScreen;
        public MpIPlatformScreenInfo MainWindowScreen {
            get {
                //if (MainWindowMonitorIdx < 0) {
                //    return null;
                //}
                //if (MainWindowMonitorIdx >= MpPlatformWrapper.Services.ScreenInfoCollection.Screens.Count()) {
                //    Debugger.Break();
                //    return null;
                //}
                //return MpPlatformWrapper.Services.ScreenInfoCollection.Screens.ElementAt(MainWindowMonitorIdx);
                if(_mainWindowScreen == null) {
                    _mainWindowScreen = MpPlatformWrapper.Services.ScreenInfoCollection.Screens.ElementAt(MainWindowMonitorIdx);
                }
                return _mainWindowScreen;
            }
        }



        #endregion

        #endregion

        #region Events

        //public event EventHandler? OnMainWindowOpened;

        public event EventHandler? OnMainWindowClosed;
        #endregion

        public MpAvMainWindowViewModel() : base() {
            PropertyChanged += MpAvMainWindowViewModel_PropertyChanged;
        }

        #region Public Methods

        public async Task InitializeAsync() {
            await Task.Delay(1);
        }

        public void SetupMainWindowSize(bool isOrientationChange = false) {
            switch (MainWindowOrientationType) {
                case MpMainWindowOrientationType.Top:
                case MpMainWindowOrientationType.Bottom:
                    MainWindowWidth = MainWindowScreen.WorkArea.Width;
                    if (MainWindowHeight == 0) {
                        // startup case                        
                        if (MpPrefViewModel.Instance.MainWindowInitialHeight == 0) {
                            // initial setting
                            MpPrefViewModel.Instance.MainWindowInitialHeight = MainWindowScreen.WorkArea.Height * MainWindowDefaultHorizontalHeightRatio;
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
                    MainWindowHeight = MainWindowScreen.WorkArea.Height;
                    if (MainWindowWidth == 0) {
                        // startup case                        
                        if (MpPrefViewModel.Instance.MainWindowInitialWidth == 0) {
                            // initial setting
                            MpPrefViewModel.Instance.MainWindowInitialWidth = MainWindowScreen.WorkArea.Width * MainWindowDefaultVerticalWidthRatio;
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
            switch (e.PropertyName) {
                case nameof(IsDropOverMainWindow):
                    MpConsole.WriteLine("IsDropOverMainWindow: " + (IsDropOverMainWindow ? "YES" : "NO"));
                    break;
                case nameof(IsHovering):
                    MpConsole.WriteLine("MainWindow Hover: " + (IsHovering ? "TRUE" : "FALSE"));
                    break;
                case nameof(LastMainWindowRect):
                    //MpConsole.WriteLine("Last mwr " + LastMainWindowRect);
                    // MpConsole.WriteLine("Cur mwr" + MainWindowRect);
                    break;
                case nameof(MainWindowHeight):
                    if (!IsResizing) {
                        return;
                    }
                    MainWindowTop = MainWindowOpenedScreenRect.Top;
                    MainWindowBottom = MainWindowOpenedScreenRect.Bottom;

                    //MpMessenger.SendGlobal<MpMessageType>(MpMessageType.MainWindowSizeChanged);
                    break;
                case nameof(MainWindowWidth):
                    if (!IsResizing) {
                        return;
                    }
                    MainWindowLeft = MainWindowOpenedScreenRect.Left;
                    MainWindowRight = MainWindowOpenedScreenRect.Right;
                    //MpAvTagTrayViewModel.Instance.OnPropertyChanged(nameof(MpAvTagTrayViewModel.Instance.TagTrayScreenWidth));
                    //MpMessenger.SendGlobal(MpMessageType.MainWindowSizeChanged);
                    break;
                case nameof(MainWindowLeft):
                case nameof(MainWindowTop):
                    if (MpAvMainWindow.Instance == null) {
                        return;
                    }
                    var p = new MpPoint(MainWindowLeft, MainWindowTop);
                    if (OperatingSystem.IsWindows()) {
                        // Window position on windows uses actual density not scaled value mac uses scaled haven't checked linux
                        p *= MainWindowScreen.PixelDensity; //MpPlatformWrapper.Services.ScreenInfoCollection.Screens.ElementAt(MainWindowMonitorIdx).PixelDensity;
                    }

                    MpAvMainWindow.Instance.Position = new PixelPoint((int)p.X, (int)p.Y);
                    break;
                case nameof(IsResizing):
                    if (IsResizing) {
                        MpMessenger.SendGlobal(MpMessageType.MainWindowSizeChangeBegin);
                    } else {
                        // after resizing store new resized dimension for next load                        

                        if (MainWindowOrientationType == MpMainWindowOrientationType.Left ||
                            MainWindowOrientationType == MpMainWindowOrientationType.Right) {
                            MpPrefViewModel.Instance.MainWindowInitialWidth = MainWindowWidth;
                        } else {
                            MpPrefViewModel.Instance.MainWindowInitialHeight = MainWindowHeight;
                        }
                        Dispatcher.UIThread.Post(async () => {
                            await Task.Delay(300);
                            MpMessenger.SendGlobal(MpMessageType.MainWindowSizeChangeEnd);
                        });
                    }
                    break;
                case nameof(IsMainWindowLocked):
                    MpAvMainWindow.Instance.Topmost = IsMainWindowLocked;
                    break;
                case nameof(IsMainWindowActive):
                    if(IsMainWindowActive) {
                        MpMessenger.SendGlobal(MpMessageType.MainWindowActivated);
                        // NOTE do NOT show window on activate to animate close it temporariliy activates 
                    } else {
                        MpMessenger.SendGlobal(MpMessageType.MainWindowDeactivated);
                        HideWindowCommand.Execute(null);
                    }
                    break;
                case nameof(DragMouseMainWindowLocation):
                    if(DragMouseMainWindowLocation == null) {
                        IsDropOverMainWindow = false;
                        return;
                    }
                    IsDropOverMainWindow = MainWindowRect.Contains(DragMouseMainWindowLocation);
                    if(!IsDropOverMainWindow) {
                        DragMouseMainWindowLocation = null;
                    }
                    break;
            }
        }
        private void ValidateWindowState() {
            Dispatcher.UIThread.Post(() => {
                //if(MpAvMainWindow.Instance.IsVisible && (!IsMainWindowOpening || !IsMainWindowClosing) {
                //    Debugger.Break();
                //}
                if(IsMainWindowOpen && (IsMainWindowClosing || IsMainWindowOpening)) {
                    Debugger.Break();
                }
            });
        }
        #endregion

        #region Commands        

        public ICommand ShowWindowCommand => new MpAsyncCommand(
            async () => {
                if (!Dispatcher.UIThread.CheckAccess()) {
                    Dispatcher.UIThread.Post(() => {
                        ShowWindowCommand.Execute(null);
                    });
                    return;
                }

                SetupMainWindowSize();

                IsMainWindowOpening = true;

                //MpMessenger.SendGlobal<MpMessageType>(MpMessageType.MainWindowOpening);

                var mw = MpAvMainWindow.Instance;
                while (mw == null) {
                    await Task.Delay(100);
                    mw = MpAvMainWindow.Instance;
                }
                mw.Show();
                mw.IsVisible = true;
                mw.Activate();

                // NOTE on windows setting Topmost= true here makes mw in animate in front of taskbar
                mw.Topmost = false;

                MpRect openStartRect = MainWindowClosedScreenRect;
                if (IsMainWindowClosing) {
                    IsMainWindowClosing = false;
                    openStartRect = MainWindowRect;
                }

                MainWindowLeft = openStartRect.Left;
                MainWindowTop = openStartRect.Top;
                MainWindowRight = openStartRect.Right;
                MainWindowBottom = openStartRect.Bottom;

                if (IsMainWindowInitiallyOpening) {
                    //while (!MpAvClipTrayViewModel.Instance.IsAllTileViewsLoaded) {
                    //    await Task.Delay(100);
                    //}
                    IsMainWindowInitiallyOpening = false;
                }

                //MpConsole.WriteLine($"SHOW WINDOW START: L: " + MainWindowLeft + " T: " + MainWindowTop + " R:" + MainWindowRight + " B:" + MainWindowBottom);

                MpRect openEndRect = MainWindowOpenedScreenRect;

                double test = MpPrefViewModel.Instance.ShowMainWindowAnimationMilliseconds;
                double tt = 500;
                double fps = 30;
                double step = tt / (fps);

                double d_l = (openEndRect.Left - openStartRect.Left) / step;
                double d_t = (openEndRect.Top - openStartRect.Top) / step;
                double d_r = (openEndRect.Right - openStartRect.Right) / step;
                double d_b = (openEndRect.Bottom - openStartRect.Bottom) / step;

                //MpConsole.WriteLine($"SHOW WINDOW STEP: L: " + d_l + " T: " + d_t + " R:" + d_r + " B:" + d_b);

                var timer = new DispatcherTimer(DispatcherPriority.Normal);
                timer.Interval = TimeSpan.FromMilliseconds(fps);

                timer.Tick += (s, e32) => {
                    if (IsMainWindowClosing) {
                        timer.Stop();
                        IsMainWindowOpening = false;
                        return;
                    }
                    bool isDone = MainWindowOrientationType switch {
                        MpMainWindowOrientationType.Bottom => MainWindowTop < openEndRect.Top,
                        MpMainWindowOrientationType.Top => MainWindowTop > openEndRect.Top,
                        MpMainWindowOrientationType.Left => MainWindowLeft > openEndRect.Left,
                        MpMainWindowOrientationType.Right => MainWindowLeft < openEndRect.Left,
                        _ => false
                    };
                    if (isDone) {
                        //MpConsole.WriteLine("SHOW WINDOW DONE");
                        MainWindowLeft = openEndRect.Left;
                        MainWindowTop = openEndRect.Top;
                        MainWindowRight = openEndRect.Right;
                        MainWindowBottom = openEndRect.Bottom;

                        timer.Stop();

                        MpAvMainWindow.Instance.Topmost = true;
                        IsMainWindowLoading = false;
                        IsMainWindowOpen = true;
                        IsMainWindowOpening = false;
                        ValidateWindowState();

                        //OnPropertyChanged(nameof(IsMainWindowOpen));
                        OnPropertyChanged(nameof(ExternalRect));
                        //OnMainWindowOpened?.Invoke(this, EventArgs.Empty);
                        MpMessenger.SendGlobal(MpMessageType.MainWindowOpened);
                    } else {
                        
                        //MpConsole.WriteLine($"SHOW WINDOW ANIMATING: L: " + MainWindowLeft + " T: " + MainWindowTop + " R:" + MainWindowRight + " B:" + MainWindowBottom);
                        //MainWindowLeft = Math.Min(openEndRect.Left,MainWindowLeft + d_l);
                        //MainWindowTop = Math.Min(openEndRect.Top,MainWindowTop + d_t);
                        //MainWindowRight = Math.Min(openEndRect.Right,MainWindowRight + d_r);
                        //MainWindowBottom = Math.Min(openEndRect.Bottom,MainWindowBottom + d_b);

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
                bool canShow = !IsMainWindowLoading &&
                        !IsShowingDialog &&
                        //!IsMainWindowOpen &&
                        !IsMainWindowClosing &&
                        !IsMainWindowOpening;
                if(!canShow) {
                    ValidateWindowState();
                }
                return canShow;
            });

        public ICommand HideWindowCommand => new MpAsyncCommand(
            async() => {
                if (!Dispatcher.UIThread.CheckAccess()) {
                    Dispatcher.UIThread.Post(() => {
                        HideWindowCommand.Execute(null);
                    });
                    return;
                }

                MpRect closeStartRect = MainWindowOpenedScreenRect;
                MpRect closeEndRect = MainWindowClosedScreenRect;

                // Hide START Events - start


                if (IsMainWindowOpening) {
                    // this occurs when mw is opening and window is deactivated, either by clicking
                    // off or alt-tab, etc. w/o accounting for the deactivate mw will open
                    // and stick until activated/deactivated

                    // to deal with just move to closed rect and hide (won't animate on windows right)

                    IsMainWindowClosing = true;
                    while(IsMainWindowOpening) {
                        await Task.Delay(20);
                    }

                    MainWindowLeft = closeEndRect.Left;
                    MainWindowTop = closeEndRect.Top;
                    MainWindowRight = closeEndRect.Right;
                    MainWindowBottom = closeEndRect.Bottom;

                    IsMainWindowLocked = false;
                    IsMainWindowClosing = false;
                    IsMainWindowOpen = false;

                    MpAvMainWindow.Instance.IsVisible = false;
                    MpAvMainWindow.Instance.Topmost = false;
                    //MpAvMainWindow.Instance.Hide();

                    // Hide END - end
                    OnPropertyChanged(nameof(IsMainWindowOpen));
                    OnPropertyChanged(nameof(ExternalRect));

                    ValidateWindowState();

                    OnMainWindowClosed?.Invoke(this, EventArgs.Empty);
                    MpMessenger.SendGlobal(MpMessageType.MainWindowHid);
                    return;
                }

                IsMainWindowClosing = true;

                MpAvMainWindow.Instance.Activate(); // must activate to animate (on windows at least)
                MpAvMainWindow.Instance.Topmost = false;

                // Hide START Events - end


                MainWindowLeft = closeStartRect.Left;
                MainWindowTop = closeStartRect.Top;
                MainWindowRight = closeStartRect.Right;
                MainWindowBottom = closeStartRect.Bottom;


                double tt = MpPrefViewModel.Instance.HideMainWindowAnimationMilliseconds;
                const double fps = 30;
                double step = tt / (fps);

                double d_l = (closeEndRect.Left - closeStartRect.Left) / step;
                double d_t = (closeEndRect.Top - closeStartRect.Top) / step;
                double d_r = (closeEndRect.Right - closeStartRect.Right) / step;
                double d_b = (closeEndRect.Bottom - closeStartRect.Bottom) / step;

                var timer = new DispatcherTimer(DispatcherPriority.Normal) {
                    Interval = TimeSpan.FromMilliseconds(fps)
                };

                timer.Tick += (s, e32) => {
                    bool isDone = MainWindowOrientationType switch {
                        MpMainWindowOrientationType.Bottom => MainWindowTop > closeEndRect.Top,
                        MpMainWindowOrientationType.Top => MainWindowTop < closeEndRect.Top,
                        MpMainWindowOrientationType.Left => MainWindowLeft < closeEndRect.Left,
                        MpMainWindowOrientationType.Right => MainWindowLeft > closeEndRect.Left,
                        _ => false
                    };
                    if (isDone) {
                        MainWindowLeft = closeEndRect.Left;
                        MainWindowTop = closeEndRect.Top;
                        MainWindowRight = closeEndRect.Right;
                        MainWindowBottom = closeEndRect.Bottom;

                        timer.Stop();

                        IsMainWindowLocked = false;
                        IsMainWindowOpen = false;
                        IsMainWindowClosing = false;


                        // Hide END - start

                        MpAvMainWindow.Instance.IsVisible = false;
                        MpAvMainWindow.Instance.Topmost = false;
                        //MpAvMainWindow.Instance.Hide();

                        // Hide END - end
                        //OnPropertyChanged(nameof(IsMainWindowOpen));
                        OnPropertyChanged(nameof(ExternalRect));

                        ValidateWindowState();

                        OnMainWindowClosed?.Invoke(this, EventArgs.Empty);
                        MpMessenger.SendGlobal(MpMessageType.MainWindowHid);
                    } else {
                        //if (IsMainWindowOpening) {
                        //    timer.Stop();
                        //    return;
                        //}
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

                bool canHide = 
                        !IsMainWindowLocked &&
                          !IsAnyDropDownOpen &&
                          !IsDropOverMainWindow &&
                          !IsShowingDialog &&
                          !IsDropOverMainWindow &&
                          //!MpContextMenuView.Instance.IsOpen &&
                          //!IsMainWindowOpening &&
                          !IsResizing &&
                          !IsMainWindowClosing;
                if(!canHide) {
                    ValidateWindowState();
                }
                return canHide;
            });

        public ICommand CycleOrientationCommand => new MpCommand<object>(
            async(dirStrOrEnumArg) => {
                int nextOr = (int)MainWindowOrientationType;

                if (dirStrOrEnumArg is string dirStr) {
                    bool isCw = dirStr.ToLower() == "cw";
                    nextOr = (int)MainWindowOrientationType + (isCw ? -1 : 1);

                    if (nextOr >= Enum.GetNames(typeof(MpMainWindowOrientationType)).Length) {
                        nextOr = 0;
                    } else if (nextOr < 0) {
                        nextOr = Enum.GetNames(typeof(MpMainWindowOrientationType)).Length - 1;
                    }
                } else if(dirStrOrEnumArg is MpMainWindowOrientationType dirEnum) {
                    nextOr = (int)dirEnum;
                }

                MpMessenger.SendGlobal(MpMessageType.MainWindowOrientationChangeBegin);

                MainWindowOrientationType = (MpMainWindowOrientationType)nextOr;
                SetupMainWindowSize(true);

                MainWindowLeft = MainWindowOpenedScreenRect.Left;
                MainWindowTop = MainWindowOpenedScreenRect.Top;
                MainWindowRight = MainWindowOpenedScreenRect.Right;
                MainWindowBottom = MainWindowOpenedScreenRect.Bottom;

                var mw = MpAvMainWindow.Instance;
                mw.UpdateContentOrientation();

                await Task.Delay(300);
                
                MpMessenger.SendGlobal(MpMessageType.MainWindowOrientationChangeEnd);
            });

        public ICommand ToggleMainWindowLockCommand => new MpCommand(
            () => {
                IsMainWindowLocked = !IsMainWindowLocked;
            });

        public ICommand ToggleFilterMenuVisibleCommand => new MpCommand(
            () => {
                IsFilterMenuVisible = !IsFilterMenuVisible;
            });

        public ICommand WindowResizeCommand => new MpCommand<MpSize>(
            (sizeArg) => {
                Dispatcher.UIThread.Post(() => {
                    var rc = MpAvMainWindow.Instance.GetResizerControl();
                    if (rc == null) {
                        return;
                    }
                    IsResizing = true;

                    MpAvResizeExtension.ResizeByDelta(rc, sizeArg.Width, sizeArg.Height);

                    IsResizing = false;
                });
            },
            (sizeArg) => {
                return IsMainWindowOpen && sizeArg != null;
            });
        
        public ICommand WindowSizeUpCommand => new MpCommand(
             () => {
                 double dir = MainWindowOrientationType == MpMainWindowOrientationType.Bottom ? 1 : -1;
                 WindowResizeCommand.Execute(new MpSize(0, _resize_shortcut_nudge_amount * dir));
             },
             () => {
                 return IsHorizontalOrientation;
                });

        public ICommand WindowSizeDownCommand => new MpCommand(
             () => {
                 double dir = MainWindowOrientationType == MpMainWindowOrientationType.Bottom ? -1 : 1;
                 WindowResizeCommand.Execute(new MpSize(0, _resize_shortcut_nudge_amount * dir));
             }, 
             () => {
                 return IsHorizontalOrientation;
             });

        public ICommand WindowSizeRightCommand => new MpCommand(
             () => {
                 double dir = MainWindowOrientationType == MpMainWindowOrientationType.Right ? -1 : 1;
                 WindowResizeCommand.Execute(new MpSize(_resize_shortcut_nudge_amount * dir,0));
             }, () => {
                 return IsVerticalOrientation;
             });

        public ICommand WindowSizeLeftCommand => new MpCommand(
             () => {
                 double dir = MainWindowOrientationType == MpMainWindowOrientationType.Right ? 1 : -1;
                 WindowResizeCommand.Execute(new MpSize(_resize_shortcut_nudge_amount * dir, 0));
             }, () => {
                 return IsVerticalOrientation;
             });

        public ICommand WindowSizeToDefaultCommand => new MpCommand(
            () => {
                var rc = MpAvMainWindow.Instance.GetResizerControl();
                if (rc == null) {
                    return;
                }
                IsResizing = true;

                MpAvResizeExtension.ResetToDefault(rc);

                IsResizing = false;
            });

        private MpCommand _undoCommand;
        public ICommand UndoCommand {
            get {
                if (_undoCommand == null) {
                    _undoCommand = new MpCommand(() => UndoManager.Undo(), () => UndoManager.CanUndo);
                }
                return _undoCommand;
            }
        }

        private ICommand _redoCommand;
        public ICommand RedoCommand {
            get {
                if (_redoCommand == null)
                    _redoCommand = new MpCommand(() => UndoManager.Redo(), () => UndoManager.CanRedo);
                return _redoCommand;
            }
        }
        #endregion

    }
}
