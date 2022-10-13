using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Threading;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Avalonia;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MonkeyPaste.Avalonia {
    public class MpAvMainWindowViewModel : MpViewModelBase, MpIResizableViewModel {
        #region Private Variables

        private double _resize_shortcut_nudge_amount = 50;

        //private DispatcherTimer _windowOpenTimer;
        //private DispatcherTimer _windowCloseTimer;
        //private double _windowTimerStep;
        //private double d_l, d_r, d_t, d_b;

        private int _animateTimeOut_ms = 5000;
        private CancellationTokenSource _animationCts;

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
        public MpRect ObservedMainWindowRect { get; set; } = new MpRect();

        public MpRect MainWindowScreenRect {
            get {
                return new MpRect(MainWindowLeft, MainWindowTop, MainWindowWidth, MainWindowHeight);
            }
            set {
                MpRect newVal = value == null ? MpRect.Empty : value;
                MainWindowWidth = newVal.Width;// - newVal;
                MainWindowHeight = newVal.Height;// - newVal;
                MainWindowLeft = newVal.Left;
                MainWindowTop = newVal.Top;
                MainWindowRight = newVal.Right;
                MainWindowBottom = newVal.Bottom;


                OnPropertyChanged(nameof(MainWindowScreenRect));
            }
        }


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

        public bool AnimateShowWindow { get; set; } = true;
        public bool AnimateHideWindow { get; set; } = true;

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

        public MpMainWindowOrientationType MainWindowOrientationType { get; private set; }
        public MpMainWindowShowBehaviorType MainWindowShowBehaviorType { get; private set; }
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
                if (_mainWindowScreen == null) {
                    _mainWindowScreen = MpPlatformWrapper.Services.ScreenInfoCollection.Screens.ElementAt(MainWindowMonitorIdx);
                }
                return _mainWindowScreen;
            }
        }



        #endregion

        #endregion

        #region Events

        //public event EventHandler? OnMainWindowOpened;

        //public event EventHandler? OnMainWindowClosed;
        #endregion

        public MpAvMainWindowViewModel() : base() {
            _animationCts = new CancellationTokenSource();
            MainWindowOrientationType = (MpMainWindowOrientationType)Enum.Parse(typeof(MpMainWindowOrientationType), MpPrefViewModel.Instance.MainWindowOrientation, false);
            MainWindowShowBehaviorType = (MpMainWindowShowBehaviorType)Enum.Parse(typeof(MpMainWindowShowBehaviorType), MpPrefViewModel.Instance.MainWindowShowBehaviorType, false);
            PropertyChanged += MpAvMainWindowViewModel_PropertyChanged;
            //InitWindowTimers();
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
                    //if (OperatingSystem.IsWindows()) 
                    {
                        // Window position on windows uses actual density not scaled value mac uses scaled haven't checked linux
                        p *= MainWindowScreen.PixelDensity; //MpPlatformWrapper.Services.ScreenInfoCollection.Screens.ElementAt(MainWindowMonitorIdx).PixelDensity;
                    }

                    MpAvMainWindow.Instance.Position = p.ToAvPixelPoint();
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
                    IsDropOverMainWindow = ObservedMainWindowRect.Contains(DragMouseMainWindowLocation);
                    if(!IsDropOverMainWindow) {
                        DragMouseMainWindowLocation = null;
                    }
                    break;
                case nameof(MainWindowOrientationType):
                    MpPrefViewModel.Instance.MainWindowOrientation = MainWindowOrientationType.ToString();
                    break;
                case nameof(MainWindowShowBehaviorType):
                    MpPrefViewModel.Instance.MainWindowShowBehaviorType = MainWindowShowBehaviorType.ToString();
                    break;
            }
        }

        private void FinishMainWindowShow() {            
            if (_animationCts.IsCancellationRequested) {
                MpConsole.WriteLine("FinishShow canceled, ignoring view changes");
                return;
            }

            IsMainWindowInitiallyOpening = false;
            IsMainWindowLoading = false;
            IsMainWindowOpen = true;
            IsMainWindowOpening = false;

            MpAvMainWindow.Instance.Show();
            MpAvMainWindow.Instance.Topmost = true;
            IsMainWindowVisible = true;
            MainWindowScreenRect = MainWindowOpenedScreenRect;

            MpMessenger.SendGlobal(MpMessageType.MainWindowOpened);
            MpConsole.WriteLine("SHOW WINDOW DONE");
        }
        
        public void FinishMainWindowHide(MpPortableProcessInfo active_pinfo) {

            if(_animationCts.IsCancellationRequested) {
                MpConsole.WriteLine("FinishHide canceled, ignoring view changes");
                return;
            }

            IsMainWindowLocked = false;
            IsMainWindowOpen = false;
            IsMainWindowClosing = false;

            MainWindowScreenRect = MainWindowClosedScreenRect;
            IsMainWindowVisible = false;
            MpAvMainWindow.Instance.Hide();
            MpAvMainWindow.Instance.Topmost = false;
            //MpAvMainWindow.Instance.Renderer.Stop();

            //OnMainWindowClosed?.Invoke(this, EventArgs.Empty);
            MpMessenger.SendGlobal(MpMessageType.MainWindowHid);

            if(active_pinfo != null) {
                MpPlatformWrapper.Services.ProcessWatcher.SetActiveProcess(active_pinfo.Handle);
            }
            
            MpConsole.WriteLine("CLOSE WINDOW DONE");

            var active_info = MpPlatformWrapper.Services.ProcessWatcher.LastProcessInfo;
            MpConsole.WriteLine("Active: " + active_info);
        }
        private async Task AnimateMainWindowAsync(MpRect startRect, MpRect endRect, CancellationToken ct) {
            // close 0.12 20
            // open 
            double zeta = 0.22d;
            double omega = 25;
            if(MpAvSearchBoxViewModel.Instance.HasText) {
                var st_parts = MpAvSearchBoxViewModel.Instance.SearchText.Split(",");
                zeta = double.Parse(st_parts[0]);
                omega = double.Parse(st_parts[1]);
            }
            MainWindowScreenRect = startRect;
            double[] x = startRect.Sides;
            double[] xt = endRect.Sides;
            double[] v = new double[4];

            int anchor_idx = MainWindowOrientationType switch {
                MpMainWindowOrientationType.Left => 0,
                MpMainWindowOrientationType.Top => 1,
                MpMainWindowOrientationType.Right => 2,
                MpMainWindowOrientationType.Bottom => 3,
                _ => throw new NotImplementedException()
            };

            bool isDone = false;
            DateTime prevTime = DateTime.Now;
            EventHandler tick = (s, e) => {
                var curTime = DateTime.Now;
                double dt = (curTime - prevTime).TotalMilliseconds / 1000.0d;
                prevTime = curTime;
                for (int i = 0; i < x.Length; i++) {
                    if(i == anchor_idx) {
                        // anchor_idx is 'critically dampened' to 1 so it does not oscillate (doesn't animate past screen edge)
                        Spring(ref x[i], ref v[i], xt[i], 1, omega, dt);
                    } else {
                        Spring(ref x[i], ref v[i], xt[i], zeta, omega, dt);
                    }
                }
                MainWindowScreenRect = new MpRect(x);

                bool is_v_zero = v.All(x => Math.Abs(x) < 0.1d);

                if(is_v_zero || ct.IsCancellationRequested) {
                    // consider done when all v's are pretty low or canceled
                    isDone = true;
                    return;
                }               
            };

            var timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds(1000d / 60d);
            timer.Tick += tick;
            timer.Start();

            while(!isDone) {                
                await Task.Delay(5);
            }
            timer.Stop();
            timer.Tick -= tick;

            if(ct.IsCancellationRequested) {
                return;
            }

            MainWindowScreenRect = endRect;
        }

        private void Spring(ref double x, ref double v, double xt, double zeta, double omega, double h) {
            /*
                from https://allenchou.net/2015/04/game-math-precise-control-over-numeric-springing/
              x     - value             (input/output)
              v     - velocity          (input/output)
              xt    - target value      (input)
              zeta  - damping ratio     (input)
              omega - angular frequency (input)
              h     - time step         (input)
            */
            double f = 1.0d + 2.0d * h * zeta * omega;
            double oo = omega * omega;
            double hoo = h * oo;
            double hhoo = h * hoo;
            double detInv = 1.0d / (f + hhoo);
            double detX = f * x + h * v + hhoo * xt;
            double detV = v + hoo * (xt - x);
            x = detX * detInv;
            v = detV * detInv;
        }

        private void ClampAnchoredSide(ref double side, ref double v, int idx) {
            if(!IsMainWindowOpening) {
                return;
            }
            var s = MainWindowScreen;
            switch (MainWindowOrientationType) {
                case MpMainWindowOrientationType.Left:
                    side = idx != 0 ? side : Math.Min(s.WorkArea.Left, side);
                    break;
                case MpMainWindowOrientationType.Top:
                    side = idx != 1 ? side : Math.Min(s.WorkArea.Top, side);
                    break;
                case MpMainWindowOrientationType.Right:
                    side = idx != 2 ? side : Math.Max(s.WorkArea.Right, side);
                    break;
                case MpMainWindowOrientationType.Bottom:
                    if (side < s.WorkArea.Bottom && idx == 3) {
                        side = s.WorkArea.Bottom;
                        v = 0;
                    } 
                    //side = Math.Max(s.WorkArea.Bottom, side);
                    break;
            }
        }

        private double[] ClampAnchoredSide(double[] sides) {
            if (!IsMainWindowOpening) {
                return sides;
            }
            var s = MainWindowScreen;
            switch (MainWindowOrientationType) {
                case MpMainWindowOrientationType.Left:
                    sides[0] = Math.Min(s.WorkArea.Left, sides[0]);
                    break;
                case MpMainWindowOrientationType.Top:
                    sides[1] = Math.Min(s.WorkArea.Top, sides[1]);
                    break;
                case MpMainWindowOrientationType.Right:
                    sides[2] =Math.Max(s.WorkArea.Right, sides[2]);
                    break;
                case MpMainWindowOrientationType.Bottom:
                    if (sides[3] < s.WorkArea.Bottom) {
                        double diff = s.WorkArea.Bottom - sides[3];
                        sides[3] = s.WorkArea.Bottom;
                        sides[1] -= diff;
                        //v = 0;
                    }
                    //side = Math.Max(s.WorkArea.Bottom, side);
                    break;
            }
            return sides;
        }
        #endregion

        #region Commands        

        public ICommand ShowWindowCommand => new MpAsyncCommand(
            async () => {
                MpConsole.WriteLine("Opening Main Widow");
                if (!Dispatcher.UIThread.CheckAccess()) {
                    Dispatcher.UIThread.Post(() => {
                        ShowWindowCommand.Execute(null);
                    });
                    return;
                }
                if (IsMainWindowClosing) {
                    _animationCts?.Cancel();
                    //while (IsMainWindowClosing) { await Task.Delay(5); }
                }

                SetupMainWindowSize();

                if (IsMainWindowInitiallyOpening) {
                    MainWindowScreenRect = MainWindowClosedScreenRect;
                }

                IsMainWindowOpening = true;              

                if (AnimateShowWindow) {
                    MpAvMainWindow.Instance.Show();
                    MpAvMainWindow.Instance.Topmost = false;
                    //MpAvMainWindow.Instance.Renderer.Start();

                    _animationCts.TryReset();
                    await AnimateMainWindowAsync(MainWindowScreenRect, MainWindowOpenedScreenRect, _animationCts.Token);
                }
                FinishMainWindowShow();
            },
            () => {
                bool canShow = !IsMainWindowLoading &&
                        !IsShowingDialog &&
                        !IsMainWindowOpen &&
                        //!IsMainWindowClosing &&
                        !IsMainWindowOpening;
                if(!canShow) {
                    MpConsole.WriteLine("");
                    MpConsole.WriteLine($"Cannot show main window:");
                    MpConsole.WriteLine($"IsMainWindowOpen: {(IsMainWindowOpen)}");
                    MpConsole.WriteLine($"IsMainWindowLoading: {(IsMainWindowLoading)}");
                    MpConsole.WriteLine($"IsShowingDialog: {(IsShowingDialog)}");
                    MpConsole.WriteLine($"IsMainWindowClosing: {(IsMainWindowClosing)}");
                    MpConsole.WriteLine($"IsMainWindowOpening: {(IsMainWindowOpening)}");
                    MpConsole.WriteLine("");
                    if(IsMainWindowInitiallyOpening) {
                        return canShow;
                    }

                    if(IsMainWindowOpening) {
                        canShow = _animationCts.TryReset();
                        MpConsole.WriteLine("Canceling opening: " + canShow);
                    }
                    if (IsMainWindowClosing) {
                        canShow = _animationCts.TryReset();
                        MpConsole.WriteLine("Canceling closing: " + canShow);
                    }
                }
                return canShow;
            });

        public ICommand HideWindowCommand => new MpAsyncCommand(
            async() => {
                MpConsole.WriteLine("Closing Main WIndow");

                if (!Dispatcher.UIThread.CheckAccess()) {
                    Dispatcher.UIThread.Post(() => {
                        HideWindowCommand.Execute(null);
                    });
                    return;
                }
                if(IsMainWindowOpening) {
                    _animationCts?.Cancel();
                    //while(IsMainWindowOpening) { await Task.Delay(5); }
                }
                

                IsMainWindowClosing = true;

                MpPortableProcessInfo active_pinfo = null;
                if(!MpAvClipTrayViewModel.Instance.IsPasting) {
                    // let external paste handler sets active after hide signal because when pasting the activated app may not be last active 
                    active_pinfo = MpPlatformWrapper.Services.ProcessWatcher.LastProcessInfo;
                }
                if(AnimateHideWindow) {
                    if (!MpAvClipTrayViewModel.Instance.IsPasting) {
                        // let external paste handler sets active after hide signal because when pasting the activated app may not be last active 
                        MpPlatformWrapper.Services.ProcessWatcher.SetActiveProcess(MpPlatformWrapper.Services.ProcessWatcher.ThisAppHandle);
                    }
                    //
                    MpAvMainWindow.Instance.Topmost = false;
                    //MpAvMainWindow.Instance.Renderer.Start();
                    _animationCts.TryReset();
                    await AnimateMainWindowAsync(MainWindowScreenRect, MainWindowClosedScreenRect, _animationCts.Token);
                }
                FinishMainWindowHide(active_pinfo);
            },
            () => {

                bool canHide = 
                        !IsMainWindowLocked &&
                          !IsAnyDropDownOpen &&
                          !IsDropOverMainWindow &&
                          !IsMainWindowInitiallyOpening &&
                          !IsShowingDialog &&                          
                          !IsDropOverMainWindow &&
                          //!MpContextMenuView.Instance.IsOpen &&
                          //!IsMainWindowOpening &&
                          !IsResizing &&
                          !IsMainWindowClosing;
                if(!canHide) {
                    MpConsole.WriteLine("");
                    MpConsole.WriteLine($"Cannot hide main window:");
                    MpConsole.WriteLine($"IsMainWindowLocked: {(IsMainWindowLocked)}");
                    MpConsole.WriteLine($"IsMainWindowInitiallyOpening: {(IsMainWindowInitiallyOpening)}");
                    MpConsole.WriteLine($"IsAnyDropDownOpen: {(IsAnyDropDownOpen)}");
                    MpConsole.WriteLine($"IsDropOverMainWindow: {(IsDropOverMainWindow)}");
                    MpConsole.WriteLine($"IsShowingDialog: {(IsShowingDialog)}");
                    MpConsole.WriteLine($"IsResizing: {(IsResizing)}");
                    MpConsole.WriteLine($"IsMainWindowClosing: {(IsMainWindowClosing)}");
                    MpConsole.WriteLine("");
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
