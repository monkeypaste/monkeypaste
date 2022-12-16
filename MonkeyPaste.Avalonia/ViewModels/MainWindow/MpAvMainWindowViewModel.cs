using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
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
using FocusManager = Avalonia.Input.FocusManager;

namespace MonkeyPaste.Avalonia {
    public class MpAvMainWindowViewModel : MpViewModelBase, MpIResizableViewModel {
        #region Private Variables

        private double _resize_shortcut_nudge_amount = 50;
        //private CancellationTokenSource _animationCts;

        private bool _isAnimationCanceled = false;
        private DispatcherTimer _animationTimer;

        private const int _ANIMATE_WINDOW_TIMEOUT_MS = 2000;
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

                if (MpAvClipboardHandlerCollectionViewModel.Instance.IsSidebarVisible) {
                    return MpAvClipboardHandlerCollectionViewModel.Instance;
                }

                if (MpAvAnalyticItemCollectionViewModel.Instance.IsSidebarVisible) {
                    return MpAvAnalyticItemCollectionViewModel.Instance;
                }
                if (MpAvActionCollectionViewModel.Instance.IsSidebarVisible) {
                    return MpAvActionCollectionViewModel.Instance;
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

        public string ShowOrHideLabel => IsMainWindowOpen ? "Hide" : "Show";
        public string ShowOrHideIconResourceKey => IsMainWindowOpen ? "ClosedEyeImage" : "OpenEyeImage";
        public bool AnimateShowWindow { get; set; } = true;
        public bool AnimateHideWindow { get; set; } = true;

        public DateTime? LastDecreasedFocusLevelDateTime { get; set; }
        public bool IsAnyItemDragging {
            get {
                // TODO this only contains clip tiles now but should be the central
                // check for dnd state
                if(IsMainWindowOrientationDragging) {
                    return true;
                }
                //return MpAvClipTrayViewModel.Instance.IsAnyTileDragging;
                return MpAvDocumentDragHelper.IsDragging;
            }
        }
        public bool IsMainWindowOrientationDragging { get; set; } = false;
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

        public bool IsAnyMainWindowTextBoxFocused { get; set; }
        public bool IsAnyNotificationActivating { get; set; }

        public bool IsAnyDropDownOpen { get; set; }

        public bool IsAnyDialogOpen { get; set; } = false;

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

        #region Constructors
        private MpAvMainWindowViewModel() : base() {
            //_animationCts = new CancellationTokenSource();
            MainWindowOrientationType = (MpMainWindowOrientationType)Enum.Parse(typeof(MpMainWindowOrientationType), MpPrefViewModel.Instance.MainWindowOrientation, false);
            MainWindowShowBehaviorType = (MpMainWindowShowBehaviorType)Enum.Parse(typeof(MpMainWindowShowBehaviorType), MpPrefViewModel.Instance.MainWindowShowBehaviorType, false);
            PropertyChanged += MpAvMainWindowViewModel_PropertyChanged;
            MpAvShortcutCollectionViewModel.Instance.OnGlobalMouseReleased += Instance_OnGlobalMouseReleased;
            MpAvShortcutCollectionViewModel.Instance.OnGlobalMouseMove += Instance_OnGlobalMouseMove;
            MpAvShortcutCollectionViewModel.Instance.OnGlobalMouseClicked += Instance_OnGlobalMouseClicked;
            MpAvShortcutCollectionViewModel.Instance.OnGlobalMouseWheelScroll += Instance_OnGlobalMouseWheelScroll;
            //InitWindowTimers();
        }

        #endregion


        #region Public Methods

        public async Task InitializeAsync() {
            MpMessenger.SendGlobal(MpMessageType.MainWindowSizeChanged);

            IsMainWindowLoading = false;

            MpPlatformWrapper.Services.ClipboardMonitor.StartMonitor();

            SetupMainWindowSize();
            MainWindowScreenRect = MainWindowClosedScreenRect;

            ShowWindowCommand.Execute(null);

            // Need to delay or resizer thinks bounds are empty on initial show
            await Task.Delay(300);
            CycleOrientationCommand.Execute(null);

            MpMessenger.SendGlobal(MpMessageType.MainWindowLoadComplete);

            while (IsMainWindowInitiallyOpening) {
                await Task.Delay(100);
            }

            MpAvQueryInfoViewModel.Current.RestoreProviderValues();
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
                case nameof(IsHovering):
                    MpConsole.WriteLine("MainWindow Hover: " + (IsHovering ? "TRUE" : "FALSE"));
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
                        //p *= ; //MpPlatformWrapper.Services.ScreenInfoCollection.Screens.ElementAt(MainWindowMonitorIdx).PixelDensity;
                    }

                    MpAvMainWindow.Instance.Position = p.ToAvPixelPoint(MainWindowScreen.PixelDensity);
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
                    //MpAvMainWindow.Instance.Topmost = IsMainWindowLocked;
                    //UpdateTopmost();
                    MpMessenger.SendGlobal(IsMainWindowLocked ? MpMessageType.MainWindowLocked : MpMessageType.MainWindowUnlocked);
                    break;
                case nameof(IsMainWindowActive):
                    MpMessenger.SendGlobal(IsMainWindowActive ? MpMessageType.MainWindowActivated : MpMessageType.MainWindowDeactivated);
                    break;
                case nameof(IsMainWindowOpening):
                    MpMessenger.SendGlobal(MpMessageType.MainWindowOpening);
                    break;
                case nameof(IsMainWindowClosing):
                    MpMessenger.SendGlobal(MpMessageType.MainWindowClosing);
                    break;
                case nameof(MainWindowOrientationType):
                    MpPrefViewModel.Instance.MainWindowOrientation = MainWindowOrientationType.ToString();
                    break;
                case nameof(MainWindowShowBehaviorType):
                    MpPrefViewModel.Instance.MainWindowShowBehaviorType = MainWindowShowBehaviorType.ToString();
                    break;
            }
        }

        private void ReceivedGlobalMessage(MpMessageType msg) {
            switch(msg) {
                case MpMessageType.MainWindowDeactivated:
                    IsAnyMainWindowTextBoxFocused = false;
                    HideWindowCommand.Execute(null);
                    break;
            }
        }

        #region Window Animation Helpers

        private void FinishMainWindowShow() {            
            if (_isAnimationCanceled) {
                MpConsole.WriteLine("FinishShow canceled, ignoring view changes");
                return;
            }

            IsMainWindowInitiallyOpening = false;
            IsMainWindowLoading = false;
            IsMainWindowOpen = true;
            IsMainWindowOpening = false;

            if(!MpAvMainWindow.Instance.IsVisible) {
                // when not animated
                MpAvMainWindow.Instance.Show();
            }
            
            //if(MpAvNotificationWindow.Instance.IsVisible) {
            //    MpAvMainWindow.Instance.Topmost = false;
            //} else {
            //    MpAvMainWindow.Instance.Topmost = true;
            //}
            //MpAvMainWindow.Instance.Topmost = true;
            //UpdateTopmost();

            IsMainWindowVisible = true;
            MainWindowScreenRect = MainWindowOpenedScreenRect;

            MpMessenger.SendGlobal(MpMessageType.MainWindowOpened);
            MpConsole.WriteLine("SHOW WINDOW DONE");
        }
        
        public void FinishMainWindowHide(MpPortableProcessInfo active_pinfo) {

            if(_isAnimationCanceled) {
                MpConsole.WriteLine("FinishHide canceled, ignoring view changes");
                return;
            }

            IsMainWindowLocked = false;
            IsMainWindowOpen = false;
            IsMainWindowClosing = false;

            MainWindowScreenRect = MainWindowClosedScreenRect;
            IsMainWindowVisible = false;
            MpAvMainWindow.Instance.Hide();
            //UpdateTopmost();
            //MpAvMainWindow.Instance.Topmost = false;
            //MpAvMainWindow.Instance.Renderer.Stop();

            //OnMainWindowClosed?.Invoke(this, EventArgs.Empty);
            MpMessenger.SendGlobal(MpMessageType.MainWindowHid);

            if(active_pinfo != null) {
                //MpPlatformWrapper.Services.ProcessWatcher.SetActiveProcess(active_pinfo.Handle);
            }
            
            MpConsole.WriteLine("CLOSE WINDOW DONE");

            var active_info = MpPlatformWrapper.Services.ProcessWatcher.LastProcessInfo;
            MpConsole.WriteLine("Active: " + active_info);
        }
        private async Task AnimateMainWindowAsync(MpRect startRect, MpRect endRect) {
            // close 0.12 20
            // open 
            double zeta = 0.22d;
            double omega = 25;
            //if(MpAvSearchBoxViewModel.Instance.HasText) {
            //    var st_parts = MpAvSearchBoxViewModel.Instance.SearchText.Split(",");
            //    zeta = double.Parse(st_parts[0]);
            //    omega = double.Parse(st_parts[1]);
            //}
            //MainWindowScreenRect = startRect;
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
            if (_animationTimer == null) {
                _animationTimer = new DispatcherTimer();
                _animationTimer.Interval = TimeSpan.FromMilliseconds(1000d / 60d);
            }
            EventHandler tick = (s, e) => {
                var curTime = DateTime.Now;
                double dt = (curTime - prevTime).TotalMilliseconds / 1000.0d;
                prevTime = curTime;
                for (int i = 0; i < x.Length; i++) {
                    if(i == anchor_idx) {
                        // anchor_idx is 'critically dampened' to 1 so it does not oscillate (doesn't animate past screen edge)
                        MpAnimationHelpers.Spring(ref x[i], ref v[i], xt[i],dt, 1, omega);
                    } else {
                        MpAnimationHelpers.Spring(ref x[i], ref v[i], xt[i],dt, zeta, omega);
                    }
                }
                bool is_v_zero = v.All(x => Math.Abs(x) < 0.1d);

                if (is_v_zero || _isAnimationCanceled) {
                    // consider done when all v's are pretty low or canceled
                    isDone = true;
                    _animationTimer.Stop();
                    return;
                }
                MainWindowScreenRect = new MpRect(x);

                            
            };

            _animationTimer.Tick += tick;
            _animationTimer.Start();

            var timeout_sw = Stopwatch.StartNew();
            while (!isDone) {                
                await Task.Delay(5);
                if(timeout_sw.ElapsedMilliseconds >= _ANIMATE_WINDOW_TIMEOUT_MS) {
                    isDone = true;
                }
            }
            _animationTimer.Stop();
            _animationTimer.Tick -= tick;

            if(_isAnimationCanceled) {
                return;
            }

            MainWindowScreenRect = endRect;
        }

        private async Task ResetMainWindowAnimationStateAsync() {
            if (IsMainWindowClosing || IsMainWindowOpening) {
                _isAnimationCanceled = true;
                var sw = Stopwatch.StartNew();
                while (IsMainWindowAnimating()) {
                    await Task.Delay(100);
                    if (sw.ElapsedMilliseconds >= _ANIMATE_WINDOW_TIMEOUT_MS) {
                        break;
                    }
                }
                await Task.Delay(100);
                IsMainWindowClosing = false;
                IsMainWindowOpening = false;
                _isAnimationCanceled = false;
            }
        }

        private bool IsMainWindowAnimating() {
            if(_animationTimer == null) {
                return false;
            }
            return _animationTimer.IsEnabled;
        }

        #endregion


        #region Global Pointer Event Handlers
        private void Instance_OnGlobalMouseWheelScroll(object sender, MpPoint delta) {
            if (!MpPrefViewModel.Instance.DoShowMainWindowWithMouseEdgeAndScrollDelta) {
                return;
            }
            if (!IsMainWindowOpening && MpBootstrapperViewModelBase.IsCoreLoaded) {
                if (MpAvShortcutCollectionViewModel.Instance.GlobalMouseLocation != null &&
                         MpAvShortcutCollectionViewModel.Instance.GlobalMouseLocation.Y <= MpPrefViewModel.Instance.ShowMainWindowMouseHitZoneHeight) {
                    // show mw on top edge scroll flick
                    ShowWindowCommand.Execute(null);
                }
            }
        }

        private void Instance_OnGlobalMouseClicked(object sender, bool isLeftButton) {
            if (MpAvMainWindow.Instance.IsActive ||
                !isLeftButton ||
                !IsMainWindowOpen ||
                IsMainWindowClosing) {
                return;
            }
            var gmavp = MpAvShortcutCollectionViewModel.Instance.GlobalMouseLocation.ToAvPoint();
            if (!MpAvMainWindow.Instance.Bounds.Contains(gmavp)) {
                // attempt to hide mw
                HideWindowCommand.Execute(null);
            }
        }

        private void Instance_OnGlobalMouseReleased(object sender, bool isLeftButton) {
            if (MpAvMainWindow.Instance == null) {
                return;
            }
            if (!IsMainWindowOpen) {
                if (MpAvClipTrayViewModel.Instance.IsAutoCopyMode) {
                    if (isLeftButton && !MpAvMainWindow.Instance.IsActive) {
                        //SimulateKeyStrokeSequence("control+c");
                        MpConsole.WriteLine("Auto copy is ON");
                    }
                }
                if (MpAvClipTrayViewModel.Instance.IsRightClickPasteMode) {
                    if (!isLeftButton && !MpAvMainWindow.Instance.IsActive) {
                        // TODO this is hacky because mouse gestures are not formally handled
                        // also app collection should be queried for custom paste cmd instead of this
                        MpAvShortcutCollectionViewModel.Instance.SimulateKeyStrokeSequenceAsync("control+v").FireAndForgetSafeAsync();
                    }
                }
            } else if (!IsMainWindowClosing &&
                      !IsMainWindowLocked &&
                      //!MpExternalDropBehavior.Instance.IsPreExternalTemplateDrop &&
                      MpAvShortcutCollectionViewModel.Instance.GlobalMouseLocation != null &&
                      MpAvShortcutCollectionViewModel.Instance.GlobalMouseLocation.Y < MainWindowTop) {
                HideWindowCommand.Execute(null);
            }
        }
        private void Instance_OnGlobalMouseMove(object sender, MpPoint gmp) {
            if (IsMainWindowOpen) {
                return;
            }
            bool isShowingMainWindow = false;
            if (MpPrefViewModel.Instance.DoShowMainWindowWithMouseEdge &&
                !MpPrefViewModel.Instance.DoShowMainWindowWithMouseEdgeAndScrollDelta) {
                if (gmp.Y <= MpPrefViewModel.Instance.ShowMainWindowMouseHitZoneHeight) {
                    // show mw when mouse is within hit zone regardless of buttons or scroll delta (probably a weird pref context) 
                    ShowWindowCommand.Execute(null);
                    isShowingMainWindow = true;
                }
            }

            if (!isShowingMainWindow &&
                MpPrefViewModel.Instance.ShowMainWindowOnDragToScreenTop) {
                if (MpAvShortcutCollectionViewModel.Instance.GlobalMouseLeftButtonDownLocation != null &&
                    gmp.Distance(MpAvShortcutCollectionViewModel.Instance.GlobalMouseLeftButtonDownLocation) >= MpAvShortcutCollectionViewModel.MIN_GLOBAL_DRAG_DIST &&
                    gmp.Y <= MpPrefViewModel.Instance.ShowMainWindowMouseHitZoneHeight) {
                    // show mw during dnd and user drags to top of screen (when pref set)
                    ShowWindowCommand.Execute(null);
                }
            }
        }

        #endregion

        #endregion

        #region Commands        

        public ICommand ShowWindowCommand => new MpCommand(
             () => {

                Dispatcher.UIThread.Post(async() => {
                    if (IsMainWindowOpening && IsMainWindowAnimating()) {
                        return;
                    }

                    await ResetMainWindowAnimationStateAsync();

                    MpConsole.WriteLine("Opening Main Widow");
                    SetupMainWindowSize();


                    IsMainWindowOpening = true;

                    if (AnimateShowWindow) {
                        MpAvMainWindow.Instance.Show();
                        //MpAvMainWindow.Instance.Topmost = false;
                        //UpdateTopmost();
                        //MpAvMainWindow.Instance.Renderer.Start();

                        //_animationCts.TryReset();
                        await AnimateMainWindowAsync(MainWindowScreenRect, MainWindowOpenedScreenRect);
                    }
                    FinishMainWindowShow();
                });
                
            },
            () => {
                bool canShow = !IsMainWindowLoading &&
                        //!IsAnyDialogOpen &&
                        !IsMainWindowOpen &&
                        //!IsMainWindowClosing &&
                        !IsMainWindowOpening;

                if(!canShow) {

                    if (IsMainWindowInitiallyOpening) {
                        return canShow;
                    }

                    if(!canShow) {
                        MpConsole.WriteLine("");
                        MpConsole.WriteLine($"Cannot show main window:");
                        MpConsole.WriteLine($"IsMainWindowOpen: {(IsMainWindowOpen)}");
                        MpConsole.WriteLine($"IsMainWindowLoading: {(IsMainWindowLoading)}");
                        //MpConsole.WriteLine($"IsShowingDialog: {(IsAnyDialogOpen)}");
                        MpConsole.WriteLine($"IsMainWindowClosing: {(IsMainWindowClosing)}");
                        MpConsole.WriteLine($"IsMainWindowOpening: {(IsMainWindowOpening)}");
                        MpConsole.WriteLine("");
                    }

                    
                }
                return canShow;
            });

        public ICommand HideWindowCommand => new MpCommand(
            () => {
                Dispatcher.UIThread.Post(async () => {
                    if (IsMainWindowClosing && IsMainWindowAnimating()) {
                        return;
                    }

                    await ResetMainWindowAnimationStateAsync();

                    MpConsole.WriteLine("Closing Main WIndow");
                    IsMainWindowClosing = true;

                    MpPortableProcessInfo active_pinfo = null;
                    if (!MpAvClipTrayViewModel.Instance.IsPasting) {
                        // let external paste handler sets active after
                        // hide signal because when pasting the activated app may not be last active 
                        active_pinfo = MpPlatformWrapper.Services.ProcessWatcher.LastProcessInfo;
                    }
                    if (AnimateHideWindow) {
                        if (!MpAvClipTrayViewModel.Instance.IsPasting) {
                            // let external paste handler sets active after
                            // hide signal because when pasting the activated app may not be last active 
                            MpPlatformWrapper.Services.ProcessWatcher.SetActiveProcess(MpPlatformWrapper.Services.ProcessWatcher.ThisAppHandle);
                        }
                        //
                        //MpAvMainWindow.Instance.Topmost = false;
                        //UpdateTopmost();
                        await AnimateMainWindowAsync(MainWindowScreenRect, MainWindowClosedScreenRect);
                    }
                    FinishMainWindowHide(active_pinfo);
                });
               

            },
            () => {
                bool wasFocusLevelJustDecreased = 
                    LastDecreasedFocusLevelDateTime.HasValue && 
                        (DateTime.Now - LastDecreasedFocusLevelDateTime.Value).TotalMilliseconds < 1000;

                bool isInputControlFocused = MpAvFocusManager.IsInputControlFocused;

                bool canHide = !isInputControlFocused &&
                        !IsMainWindowLocked &&
                          !IsAnyDropDownOpen &&
                          !IsAnyMainWindowTextBoxFocused &&
                          !IsMainWindowInitiallyOpening &&
                          !IsAnyDialogOpen &&
                          !IsAnyItemDragging &&
                          !IsAnyNotificationActivating &&
                          !wasFocusLevelJustDecreased &&
                          !IsResizing;

                if(!canHide) {
                    MpConsole.WriteLine("");
                    MpConsole.WriteLine($"Cannot hide main window:");
                    MpConsole.WriteLine($"IsMainWindowLocked: {(IsMainWindowLocked)}");
                    MpConsole.WriteLine($"IsAnyDropDownOpen: {(IsAnyDropDownOpen)}");
                    MpConsole.WriteLine($"IsAnyDialogOpen: {(IsAnyDialogOpen)}");
                    MpConsole.WriteLine($"IsAnyTextBoxFocused: {(IsAnyMainWindowTextBoxFocused)}");
                    MpConsole.WriteLine($"IsMainWindowInitiallyOpening: {(IsMainWindowInitiallyOpening)}");
                    MpConsole.WriteLine($"IsShowingDialog: {(IsAnyDialogOpen)}");
                    MpConsole.WriteLine($"IsAnyItemDragging: {(IsAnyItemDragging)}");
                    MpConsole.WriteLine($"IsAnyNotificationActivating: {(IsAnyNotificationActivating)}");
                    MpConsole.WriteLine($"wasFocusLevelJustDecreased: {(wasFocusLevelJustDecreased)}");
                    MpConsole.WriteLine($"IsResizing: {(IsResizing)}");
                    MpConsole.WriteLine($"IsMainWindowClosing: {(IsMainWindowClosing)}");
                    MpConsole.WriteLine("");
                }
                return canHide;
            });

        public ICommand CycleOrientationCommand => new MpAsyncCommand<object>(
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
                    // messages are handled by window drag in title
                    nextOr = (int)dirEnum;
                    //isDiscreteChange = false;
                }


                MpMessenger.SendGlobal(MpMessageType.MainWindowOrientationChangeBegin);

                MainWindowOrientationType = (MpMainWindowOrientationType)nextOr;
                SetupMainWindowSize(true);

                MainWindowLeft = MainWindowOpenedScreenRect.Left;
                MainWindowTop = MainWindowOpenedScreenRect.Top;
                MainWindowRight = MainWindowOpenedScreenRect.Right;
                MainWindowBottom = MainWindowOpenedScreenRect.Bottom;

                MainWindowScreenRect = MainWindowOpenedScreenRect;


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
                    //var rc = MpAvMainWindow.Instance.GetResizerControl();
                    //if (rc == null) {
                    //    return;
                    //}
                    IsResizing = true;

                    MpAvResizeExtension.ResizeByDelta(MpAvMainWindow.Instance, sizeArg.Width, sizeArg.Height);

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
                //var rc = MpAvMainWindow.Instance.GetResizerControl();
                //if (rc == null) {
                //    return;
                //}
                IsResizing = true;

                MpAvResizeExtension.ResetToDefault(MpAvMainWindow.Instance);

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

        public ICommand ToggleShowMainWindowCommand => new MpCommand(() => {
            if(IsMainWindowOpen) {
                if(IsMainWindowLocked) {
                    IsMainWindowLocked = false;
                }
                HideWindowCommand.Execute(null);
            } else {
                ShowWindowCommand.Execute(null);
            }
        }, () => !IsMainWindowLoading);
        #endregion

    }
}
