using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Platform;
using Avalonia.Threading;
using Avalonia.VisualTree;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Avalonia;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
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

        #endregion

        #region Layout

        public double AvailableContentAndSidebarWidth {
            get {
                if (IsVerticalOrientation) {
                    return MainWindowWidth -
                        MpAvMainWindowTitleMenuViewModel.Instance.TitleMenuWidth;
                }
                return MainWindowWidth -
                        MpAvSidebarItemCollectionViewModel.Instance.ButtonGroupFixedDimensionLength;
            }
        }

        public double AvailableContentAndSidebarHeight {
            get {
                if (IsVerticalOrientation) {
                    return MainWindowHeight -
                        //MpAvMainWindowTitleMenuViewModel.Instance.TitleMenuHeight -
                        MpAvSearchCriteriaItemCollectionViewModel.Instance.BoundCriteriaListBoxScreenHeight -
                        MpAvFilterMenuViewModel.Instance.FilterMenuHeight -
                        MpAvSidebarItemCollectionViewModel.Instance.ButtonGroupFixedDimensionLength;
                }
                return MainWindowHeight -
                        MpAvMainWindowTitleMenuViewModel.Instance.TitleMenuHeight -
                        MpAvSearchCriteriaItemCollectionViewModel.Instance.BoundCriteriaListBoxScreenHeight -
                        MpAvFilterMenuViewModel.Instance.FilterMenuHeight;
            }
        }

        public double MainWindowDefaultHorizontalHeightRatio =>
#if DESKTOP
            0.35;
#else
            1.0d;
#endif

        public double MainWindowDefaultVerticalWidthRatio =>
#if DESKTOP
            0.2;
#else
            1.0d;
#endif

        public double MainWindowWidth { get; set; }
        public double MainWindowHeight { get; set; }

        public double MainWindowLeft { get; set; }

        public double MainWindowRight { get; set; }

        public double MainWindowTop { get; set; }

        public double MainWindowBottom { get; set; }

        public MpRect MainWindowScreenRect =>
            new MpRect(MainWindowLeft, MainWindowTop, MainWindowWidth, MainWindowHeight);

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

        public MpRect MainWindowOpenedScreenRect {
            get {
                if (!MpPlatform.Services.PlatformInfo.IsDesktop) {
                    return MainWindowScreen.Bounds;
                }

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
        #endregion

        #region State

        public double MainWindowTransformAngle {
            get {
#if DESKTOP
                return 0;
#else
                switch (MainWindowOrientationType) {
                    default:
                    case MpMainWindowOrientationType.Bottom:
                        return 270;
                    case MpMainWindowOrientationType.Left:
                        return 0;
                    case MpMainWindowOrientationType.Top:
                        return 90;
                    case MpMainWindowOrientationType.Right:
                        return 180;
                }
#endif
            }
        }
        public bool IsDesktop =>
            MpPlatform.Services != null &&
            MpPlatform.Services.PlatformInfo != null &&
            MpPlatform.Services.PlatformInfo.IsDesktop;

        private IEnumerable<MpIAsyncObject> _busyCheckInstances;
        public bool IsAnyBusy {
            get {
                if (IsBusy) {
                    return true;
                }
                if (_busyCheckInstances == null) {
                    _busyCheckInstances =
                        MpPlatform.Services.StartupObjectLocator
                        .Items
                        .Where(x => x is MpIAsyncObject)
                        .Cast<MpIAsyncObject>();
                }
                return
                    _busyCheckInstances.Any(x => x.IsBusy);
            }
        }
        public string ShowOrHideLabel => IsMainWindowOpen ? "Hide" : "Show";
        public string ShowOrHideIconResourceKey => IsMainWindowOpen ? "ClosedEyeImage" : "OpenEyeImage";
        public bool AnimateShowWindow { get; set; } = true;
        public bool AnimateHideWindow { get; set; } = true;

        public DateTime? LastDecreasedFocusLevelDateTime { get; set; }
        public bool IsAnyItemDragging {
            get {
                // TODO this only contains clip tiles now but should be the central
                // check for dnd state
                if (IsMainWindowOrientationDragging) {
                    return true;
                }
                //return MpAvClipTrayViewModel.Instance.IsAnyTileDragging;
                return MpAvDocumentDragHelper.IsDragging;
            }
        }
        public bool IsMainWindowOrientationDragging { get; set; } = false;
        public bool IsHovering { get; set; }
        public bool IsMainWindowInitiallyOpening { get; set; } = true;
        public bool IsMainWindowOpening { get; private set; }
        public bool IsMainWindowClosing { get; private set; }
        public bool IsMainWindowOpen { get; private set; } = false;
        public bool IsMainWindowVisible { get; set; }
        public bool IsMainWindowLoading { get; set; } = true;

        private bool _isMainWindowLocked;
        public bool IsMainWindowLocked {
            get {
                if (IsMainWindowSilentLocked) {
                    return true;
                }
                return _isMainWindowLocked;
            }
            set {
                if (_isMainWindowLocked != value) {
                    _isMainWindowLocked = value;
                    OnPropertyChanged(nameof(IsMainWindowLocked));
                }
            }
        }
        public bool IsMainWindowSilentLocked { get; set; } = false;
        public bool IsResizing { get; set; } = false;
        public bool CanResize { get; set; } = false;

        private bool _isAnyMainWindowTextBoxFocused;
        public bool IsAnyMainWindowTextBoxFocused {
            get {
                if (MpAvFocusManager.Instance.IsInputControlFocused) {
                    return true;
                }
                return _isAnyMainWindowTextBoxFocused;
            }
            set {
                if (_isAnyMainWindowTextBoxFocused != value) {
                    _isAnyMainWindowTextBoxFocused = value;
                    OnPropertyChanged(nameof(IsAnyMainWindowTextBoxFocused));
                }
            }
        }
        public bool IsAnyNotificationActivating { get; set; }

        public bool IsAnyDropDownOpen { get; set; }

        public bool IsAnyDialogOpen { get; set; } = false;

        public bool IsMainWindowActive { get; set; }

        public bool IsFilterMenuVisible { get; set; } = true;

        public bool IsHorizontalOrientation =>
            MainWindowOrientationType == MpMainWindowOrientationType.Bottom ||
            MainWindowOrientationType == MpMainWindowOrientationType.Top;

        public bool IsVerticalOrientation =>
            !IsHorizontalOrientation;

        public MpMainWindowOrientationType MainWindowOrientationType { get; private set; }
        public MpMainWindowShowBehaviorType MainWindowShowBehaviorType { get; private set; }
        public int MainWindowMonitorIdx {
            get {
                switch (MainWindowShowBehaviorType) {
                    case MpMainWindowShowBehaviorType.Primary:
                    default:
                        // NOTE will need another monitor to build out non-primary display types
                        int monitorIdx = MpPlatform.Services.ScreenInfoCollection.Screens.IndexOf(x => x.IsPrimary);
                        _mainWindowScreen =
                            monitorIdx < 0 ?
                            MpPlatform.Services.ScreenInfoCollection.Screens.FirstOrDefault() :
                            MpPlatform.Services.ScreenInfoCollection.Screens.ElementAt(monitorIdx);
                        return monitorIdx;
                }
            }
        }

        private MpIPlatformScreenInfo _mainWindowScreen;
        public MpIPlatformScreenInfo MainWindowScreen {
            get {

                if (MpPlatform.Services == null ||
                    MpPlatform.Services.ScreenInfoCollection == null ||
                    MpPlatform.Services.ScreenInfoCollection.Screens == null ||
                    !MpPlatform.Services.ScreenInfoCollection.Screens.Any()) {
                    if (Application.Current.ApplicationLifetime is ISingleViewApplicationLifetime mobile
                        && mobile.MainView != null) {
                        return new MpAvDesktopScreenInfo(mobile.MainView.GetVisualRoot().AsScreen());
                    }
                    return new MpAvDesktopScreenInfo() { IsPrimary = true };
                }
                if (_mainWindowScreen == null) {
                    if (MainWindowMonitorIdx < 0 &&
                        MpPlatform.Services.ScreenInfoCollection.Screens.Any()) {

                    }
                    _mainWindowScreen = MpPlatform.Services.ScreenInfoCollection.Screens.ElementAt(MainWindowMonitorIdx);
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
#if DESKTOP
            MainWindowOrientationType = MpMainWindowOrientationType.Bottom;
#else
            MainWindowOrientationType = MpMainWindowOrientationType.Left;
#endif
            MainWindowShowBehaviorType = MpMainWindowShowBehaviorType.Primary;
            PropertyChanged += MpAvMainWindowViewModel_PropertyChanged;
        }

        #endregion

        #region Public Methods

        public async Task InitializeAsync() {
            //if (App.MainWindow != null) {
            //    App.MainWindow.DataContext = this;
            //} else {
            //    MpAvMainView.Instance.DataContext = this;
            //}

            MainWindowOrientationType = (MpMainWindowOrientationType)Enum.Parse(typeof(MpMainWindowOrientationType), MpPrefViewModel.Instance.MainWindowOrientation, false);
            MainWindowShowBehaviorType = (MpMainWindowShowBehaviorType)Enum.Parse(typeof(MpMainWindowShowBehaviorType), MpPrefViewModel.Instance.MainWindowShowBehaviorType, false);
            OnPropertyChanged(nameof(MainWindowScreen));
            OnPropertyChanged(nameof(IsDesktop));

            MpAvShortcutCollectionViewModel.Instance.OnGlobalMouseReleased += Instance_OnGlobalMouseReleased;
            MpAvShortcutCollectionViewModel.Instance.OnGlobalMouseMove += Instance_OnGlobalMouseMove;
            MpAvShortcutCollectionViewModel.Instance.OnGlobalMouseClicked += Instance_OnGlobalMouseClicked;
            MpAvShortcutCollectionViewModel.Instance.OnGlobalMouseWheelScroll += Instance_OnGlobalMouseWheelScroll;

            if (!MpPlatform.Services.PlatformInfo.IsDesktop) {
                AnimateShowWindow = false;
                AnimateHideWindow = false;
            }
            App.MainView.DataContext = this;

            MpMessenger.SendGlobal(MpMessageType.MainWindowSizeChanged);

            IsMainWindowLoading = false;

            MpPlatform.Services.ClipboardMonitor.StartMonitor();

            SetupMainWindowSize();
            SetMainWindowRect(MainWindowClosedScreenRect);

            ShowMainWindowCommand.Execute(null);

            // Need to delay or resizer thinks bounds are empty on initial show
            await Task.Delay(300);
            CycleOrientationCommand.Execute(null);

            MpMessenger.SendGlobal(MpMessageType.MainWindowLoadComplete);

            while (IsMainWindowInitiallyOpening) {
                await Task.Delay(100);
            }

            MpPlatform.Services.Query.RestoreProviderValues();
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
                    //MpAvTagTrayViewModel.Instance.OnPropertyChanged(nameof(MpAvTagTrayViewModel.Instance.MaxTagTrayScreenWidth));
                    //MpMessenger.SendGlobal(MpMessageType.MainWindowSizeChanged);
                    break;
                case nameof(MainWindowLeft):
                    double rl = MainWindowLeft - MainWindowOpenedScreenRect.Left;
                    Canvas.SetLeft(MpAvMainView.Instance.RootGrid, rl);
                    break;
                case nameof(MainWindowTop):
                    double rt = MainWindowTop - MainWindowOpenedScreenRect.Top;
                    Canvas.SetTop(MpAvMainView.Instance.RootGrid, rt);
                    break;
                case nameof(MainWindowRight):
                    double rr = MainWindowRight - MainWindowOpenedScreenRect.Right;
                    Canvas.SetRight(MpAvMainView.Instance.RootGrid, rr);
                    break;
                case nameof(MainWindowBottom):
                    double rb = MainWindowBottom - MainWindowOpenedScreenRect.Bottom;
                    Canvas.SetBottom(MpAvMainView.Instance.RootGrid, rb);
                    break;

                case nameof(MainWindowOpenedScreenRect):
                    // mw is always open screen rect
                    // mw opacity mask is always open screen rect
                    // mwcg is what is animated so it hides outside current screen workarea
                    App.MainView.SetPosition(MainWindowOpenedScreenRect.Location, MainWindowScreen.Scaling);

                    //MpAvMainView.Instance.Width = MainWindowOpenedScreenRect.Width;
                    //MpAvMainView.Instance.Height = MainWindowOpenedScreenRect.Height;

                    //OnPropertyChanged(nameof(MainWindowOpacityMask));

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
                case nameof(IsMainWindowSilentLocked):
                    OnPropertyChanged(nameof(IsMainWindowLocked));
                    break;
                case nameof(IsMainWindowLocked):
                    //MpAvMainView.Instance.Topmost = IsMainWindowLocked;
                    //UpdateTopmost();
                    MpMessenger.SendGlobal(IsMainWindowLocked ? MpMessageType.MainWindowLocked : MpMessageType.MainWindowUnlocked);
                    break;
                case nameof(IsMainWindowActive):
                    MpMessenger.SendGlobal(IsMainWindowActive ? MpMessageType.MainWindowActivated : MpMessageType.MainWindowDeactivated);
                    break;
                case nameof(IsMainWindowOpen):
                    MpMessenger.SendGlobal(IsMainWindowOpen ? MpMessageType.MainWindowOpened : MpMessageType.MainWindowClosed);
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

        #region Window Animation Helpers
        private void SetupMainWindowSize(bool isOrientationChange = false) {
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

        private void SetMainWindowRect(MpRect rect) {
            MainWindowLeft = rect.Left;
            MainWindowTop = rect.Top;
            MainWindowRight = rect.Right;
            MainWindowBottom = rect.Bottom;
        }
        private void StartMainWindowShow() {
            //MpAvMainView.Instance.Renderer.Start();
            SetupMainWindowSize();
            IsMainWindowOpening = true;

            App.MainView.Show();
            SetMainWindowRect(MainWindowOpenedScreenRect);
            //MpAvMainView.Instance.InvalidateAll();
            //if (App.MainWindow != null) {

            //App.MainWindow.Renderer.Paint(MainWindowOpenedScreenRect.ToAvRect());
            //}
            // BUG after initial show mw doesn't repaint unless resized
            //WindowSizeUpCommand.Execute(null);
            //WindowSizeDownCommand.Execute(null);
        }
        private void FinishMainWindowShow() {
            if (_isAnimationCanceled) {
                MpConsole.WriteLine("FinishShow canceled, ignoring view changes");
                return;
            }

            IsMainWindowInitiallyOpening = false;
            IsMainWindowLoading = false;
            IsMainWindowOpen = true;
            IsMainWindowOpening = false;
            //if (App.MainWindow != null) {

            //App.MainWindow.Renderer.Paint(MainWindowOpenedScreenRect.ToAvRect());
            //}

            MpConsole.WriteLine("SHOW WINDOW DONE");
        }

        public void FinishMainWindowHide(MpPortableProcessInfo active_pinfo) {

            if (_isAnimationCanceled) {
                MpConsole.WriteLine("FinishHide canceled, ignoring view changes");
                return;
            }

            IsMainWindowLocked = false;
            IsMainWindowOpen = false;
            IsMainWindowClosing = false;

            //IsMainWindowVisible = false;

            App.MainView.Hide();
            //MpAvMainView.Instance.WindowState = WindowState.Minimized;
            //MpAvMainView.Instance.Renderer.Stop();
            //SetMainWindowRect(MainWindowClosedScreenRect);

            MpConsole.WriteLine("CLOSE WINDOW DONE");
        }
        private async Task AnimateMainWindowAsync(MpRect endRect) {
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
            double[] x = new double[] { MainWindowLeft, MainWindowTop, MainWindowRight, MainWindowBottom };
            double[] xt = endRect.Sides;
            double[] v = new double[4];
            double min_done_v = 0.9d; //0.1d;
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
                    if (i == anchor_idx) {
                        // anchor_idx is 'critically dampened' to 1 so it does not oscillate (doesn't animate past screen edge)
                        MpAnimationHelpers.Spring(ref x[i], ref v[i], xt[i], dt, 1, omega);
                    } else {
                        MpAnimationHelpers.Spring(ref x[i], ref v[i], xt[i], dt, zeta, omega);
                    }
                }
                bool is_v_zero = v.All(x => Math.Abs(x) <= min_done_v);

                if (is_v_zero || _isAnimationCanceled) {
                    // consider done when all v's are pretty low or canceled
                    isDone = true;
                    _animationTimer.Stop();
                    return;
                }
                SetMainWindowRect(new MpRect(x));

            };

            _animationTimer.Tick += tick;
            _animationTimer.Start();

            var timeout_sw = Stopwatch.StartNew();
            while (!isDone) {
                await Task.Delay(5);
                if (timeout_sw.ElapsedMilliseconds >= _ANIMATE_WINDOW_TIMEOUT_MS) {
                    isDone = true;
                }
            }
            _animationTimer.Stop();
            _animationTimer.Tick -= tick;

            if (_isAnimationCanceled) {
                return;
            }

            SetMainWindowRect(endRect);
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
            if (_animationTimer == null) {
                return false;
            }
            return _animationTimer.IsEnabled;
        }

        #endregion

        #region Global Pointer Event Handlers
        private void Instance_OnGlobalMouseWheelScroll(object sender, MpPoint delta) {
            Dispatcher.UIThread.Post(() => {
                if (!MpPrefViewModel.Instance.DoShowMainWindowWithMouseEdgeAndScrollDelta) {
                    return;
                }

                bool is_core_loaded = MpPlatform.Services != null &&
                     MpPlatform.Services.StartupState != null &&
                     MpPlatform.Services.StartupState.IsCoreLoaded;

                if (!IsMainWindowOpening && is_core_loaded) {
                    if (MpAvShortcutCollectionViewModel.Instance.GlobalMouseLocation != null &&
                             MpAvShortcutCollectionViewModel.Instance.GlobalMouseLocation.Y <= MpPrefViewModel.Instance.ShowMainWindowMouseHitZoneHeight) {
                        // show mw on top edge scroll flick
                        ShowMainWindowCommand.Execute(null);
                    }
                }
            });

        }

        private void Instance_OnGlobalMouseClicked(object sender, bool isLeftButton) {
            Dispatcher.UIThread.Post(() => {
                if (App.MainView.IsActive ||
                    !isLeftButton ||
                    !IsMainWindowOpen ||
                    IsMainWindowClosing) {
                    return;
                }
                var gmavp = MpAvShortcutCollectionViewModel.Instance.GlobalMouseLocation.ToAvPoint();
                if (!MpAvMainView.Instance.Bounds.Contains(gmavp)) {
                    // attempt to hide mw
                    HideMainWindowCommand.Execute(null);
                }
            });

        }

        private void Instance_OnGlobalMouseReleased(object sender, bool isLeftButton) {
            Dispatcher.UIThread.Post(() => {
                if (MpAvMainView.Instance == null) {
                    return;
                }
                if (!IsMainWindowOpen) {
                    if (MpAvClipTrayViewModel.Instance.IsAutoCopyMode) {
                        if (isLeftButton && !App.MainView.IsActive) {
                            //SimulateKeyStrokeSequence("control+c");
                            MpConsole.WriteLine("Auto copy is ON");
                        }
                    }
                    if (MpAvClipTrayViewModel.Instance.IsRightClickPasteMode) {
                        if (!isLeftButton && !App.MainView.IsActive) {
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
                    HideMainWindowCommand.Execute(null);
                }
            });
        }
        private void Instance_OnGlobalMouseMove(object sender, MpPoint gmp) {
            Dispatcher.UIThread.Post(() => {
                if (IsMainWindowOpen) {
                    return;
                }
                bool isShowingMainWindow = false;
                if (MpPrefViewModel.Instance.DoShowMainWindowWithMouseEdge &&
                    !MpPrefViewModel.Instance.DoShowMainWindowWithMouseEdgeAndScrollDelta) {
                    if (gmp.Y <= MpPrefViewModel.Instance.ShowMainWindowMouseHitZoneHeight) {
                        // show mw when mouse is within hit zone regardless of buttons or scroll delta (probably a weird pref context) 
                        ShowMainWindowCommand.Execute(null);
                        isShowingMainWindow = true;
                    }
                }

                if (!isShowingMainWindow &&
                    MpPrefViewModel.Instance.ShowMainWindowOnDragToScreenTop) {
                    if (MpAvShortcutCollectionViewModel.Instance.GlobalMouseLeftButtonDownLocation != null &&
                        gmp.Distance(MpAvShortcutCollectionViewModel.Instance.GlobalMouseLeftButtonDownLocation) >= MpAvShortcutCollectionViewModel.MIN_GLOBAL_DRAG_DIST &&
                        gmp.Y <= MpPrefViewModel.Instance.ShowMainWindowMouseHitZoneHeight) {
                        // show mw during dnd and user drags to top of screen (when pref set)
                        // ShowMainWindowCommand.Execute(null)
                    }
                }
            });

        }

        #endregion

        #endregion

        #region Commands        

        public ICommand ShowMainWindowCommand => new MpAsyncCommand(
             async () => {
                 //Dispatcher.UIThread.VerifyAccess();
                 //Dispatcher.UIThread.Post(async () => {
                 if (IsMainWindowOpening && IsMainWindowAnimating()) {
                     return;
                 }

                 await ResetMainWindowAnimationStateAsync();

                 MpConsole.WriteLine("Opening Main Widow");

                 StartMainWindowShow();

                 if (AnimateShowWindow) {
                     await AnimateMainWindowAsync(MainWindowOpenedScreenRect);
                 }
                 FinishMainWindowShow();
                 // });

             },
            () => {
                bool canShow = !IsMainWindowLoading &&
                        //!IsAnyDialogOpen &&
                        !IsMainWindowOpen &&
                        //!IsMainWindowClosing &&
                        !IsMainWindowOpening;

                if (!canShow) {

                    if (IsMainWindowInitiallyOpening) {
                        return canShow;
                    }

                    if (!canShow) {
                        //MpConsole.WriteLine("");
                        //MpConsole.WriteLine($"Cannot show main window:");
                        //MpConsole.WriteLine($"IsMainWindowOpen: {(IsMainWindowOpen)}");
                        //MpConsole.WriteLine($"IsMainWindowLoading: {(IsMainWindowLoading)}");
                        //MpConsole.WriteLine($"IsMainWindowClosing: {(IsMainWindowClosing)}");
                        //MpConsole.WriteLine($"IsMainWindowOpening: {(IsMainWindowOpening)}");
                        //MpConsole.WriteLine("");
                    }


                }
                return canShow;
            });

        public ICommand HideMainWindowCommand => new MpAsyncCommand(
            async () => {
                Dispatcher.UIThread.VerifyAccess();
                //Dispatcher.UIThread.Post(async () => {
                if (IsMainWindowClosing && IsMainWindowAnimating()) {
                    return;
                }

                await ResetMainWindowAnimationStateAsync();

                MpConsole.WriteLine("Closing Main WIndow");
                IsMainWindowClosing = true;

                MpPortableProcessInfo active_pinfo = null;
                //if (!MpAvClipTrayViewModel.Instance.IsPasting) {
                //    // let external paste handler sets active after
                //    // hide signal because when pasting the activated app may not be last active 
                //    active_pinfo = MpPlatformWrapper.Services.ProcessWatcher.LastProcessInfo;
                //}
                if (AnimateHideWindow) {
                    //if (!MpAvClipTrayViewModel.Instance.IsPasting) {
                    //    // let external paste handler sets active after
                    //    // hide signal because when pasting the activated app may not be last active 
                    //    MpPlatformWrapper.Services.ProcessWatcher.SetActiveProcess(MpPlatformWrapper.Services.ProcessWatcher.ThisAppHandle);
                    //}
                    //
                    //MpAvMainView.Instance.Topmost = false;
                    //UpdateTopmost();
                    await AnimateMainWindowAsync(MainWindowClosedScreenRect);
                }
                FinishMainWindowHide(active_pinfo);
                //});
            },
            () => {
                if (MpPlatform.Services != null &&
                    MpPlatform.Services.PlatformInfo != null &&
                    !MpPlatform.Services.PlatformInfo.IsDesktop) {
                    return false;
                }

                bool isContextMenuOpen =
                    FocusManager.Instance.Current != null &&
                    FocusManager.Instance.Current is Control c &&
                    (c.GetVisualAncestor<ContextMenu>() != null ||
                        c.GetVisualAncestor<ComboBoxItem>() != null ||
                        (c.GetVisualAncestor<TextBox>() is TextBox tb &&
                         !tb.IsReadOnly));

                bool canHide = !IsMainWindowLocked &&
                          !IsAnyDropDownOpen &&
                          !IsMainWindowInitiallyOpening &&
                          !IsAnyDialogOpen &&
                            !isContextMenuOpen &&
                          !IsAnyItemDragging &&
                          !IsAnyNotificationActivating &&
                          !MpAvShortcutCollectionViewModel.Instance.GlobalIsMouseLeftButtonDown && // reject drag cancel event
                          !IsResizing;

                if (!canHide) {
                    //MpConsole.WriteLine("");
                    //MpConsole.WriteLine($"Cannot hide main window:");
                    //MpConsole.WriteLine($"IsMainWindowLocked: {(IsMainWindowLocked)}");
                    //MpConsole.WriteLine($"IsAnyDropDownOpen: {(IsAnyDropDownOpen)}");
                    //MpConsole.WriteLine($"IsMainWindowInitiallyOpening: {(IsMainWindowInitiallyOpening)}");
                    //MpConsole.WriteLine($"IsShowingDialog: {(IsAnyDialogOpen)}");
                    //MpConsole.WriteLine($"IsAnyItemDragging: {(IsAnyItemDragging)}");
                    //MpConsole.WriteLine($"IsAnyNotificationActivating: {(IsAnyNotificationActivating)}");
                    //MpConsole.WriteLine($"IsResizing: {(IsResizing)}");
                    //MpConsole.WriteLine($"IsMainWindowClosing: {(IsMainWindowClosing)}");
                    //MpConsole.WriteLine($"isContextMenuOpen: {(isContextMenuOpen)}");
                    //MpConsole.WriteLine("");
                }
                return canHide;
            });

        public ICommand DecreaseFocusCommand => new MpCommand(
            () => {
                HideMainWindowCommand.Execute(null);
            }, () => {
                if (!HideMainWindowCommand.CanExecute(null)) {
                    return false;
                }
                bool wasFocusLevelJustDecreased =
                    LastDecreasedFocusLevelDateTime.HasValue &&
                        (DateTime.Now - LastDecreasedFocusLevelDateTime.Value).TotalMilliseconds < 1000;


                bool canDecrease =
                    !wasFocusLevelJustDecreased &&
                          !IsAnyMainWindowTextBoxFocused;


                if (!canDecrease) {
                    MpConsole.WriteLine("");
                    MpConsole.WriteLine($"Cannot decrease focus:");
                    MpConsole.WriteLine($"IsAnyTextBoxFocused: {(IsAnyMainWindowTextBoxFocused)}");

                    MpConsole.WriteLine($"wasFocusLevelJustDecreased: {(wasFocusLevelJustDecreased)}");
                }
                return canDecrease;
            });
        public ICommand CycleOrientationCommand => new MpAsyncCommand<object>(
            async (dirStrOrEnumArg) => {
                while (MpAvMainView.Instance == null) {
                    await Task.Delay(100);
                }

                int nextOr = (int)MainWindowOrientationType;

                if (dirStrOrEnumArg is string dirStr) {
                    bool isCw = dirStr.ToLower() == "cw";
                    nextOr = (int)MainWindowOrientationType + (isCw ? -1 : 1);

                    if (nextOr >= Enum.GetNames(typeof(MpMainWindowOrientationType)).Length) {
                        nextOr = 0;
                    } else if (nextOr < 0) {
                        nextOr = Enum.GetNames(typeof(MpMainWindowOrientationType)).Length - 1;
                    }
                } else if (dirStrOrEnumArg is MpMainWindowOrientationType dirEnum) {
                    // messages are handled by window drag in title
                    nextOr = (int)dirEnum;
                    //isDiscreteChange = false;
                }

                MpMessenger.SendGlobal(MpMessageType.MainWindowOrientationChangeBegin);

                MainWindowOrientationType = (MpMainWindowOrientationType)nextOr;
                OnPropertyChanged(nameof(MainWindowTransformAngle));
                if (!MpPlatform.Services.PlatformInfo.IsDesktop) {
                    MainWindowScreen.Rotate(MainWindowTransformAngle);
                }
                OnPropertyChanged(nameof(MainWindowScreen));
                SetupMainWindowSize(true);

                SetMainWindowRect(MainWindowOpenedScreenRect);
                MpConsole.WriteLine($"MW Orientation: '{MainWindowOrientationType}' Angle: '{MainWindowTransformAngle}' Bounds: '{MainWindowScreen.Bounds}'");


                MpAvMainView.Instance.UpdateContentLayout();

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

        public ICommand WindowResizeCommand => new MpCommand<object>(
            (deltaSizeArg) => {
                Dispatcher.UIThread.Post(() => {
                    var deltaSize = deltaSizeArg as MpPoint;
                    if (deltaSize == null) {
                        return;
                    }
                    IsResizing = true;

                    MpAvResizeExtension.ResizeByDelta(MpAvMainView.Instance, deltaSize.X, deltaSize.Y);

                    IsResizing = false;
                });
            },
            (sizeArg) => {
                return IsMainWindowOpen && sizeArg != null;
            });

        public ICommand WindowSizeUpCommand => new MpCommand(
             () => {
                 double dir = MainWindowOrientationType == MpMainWindowOrientationType.Bottom ? 1 : -1;
                 WindowResizeCommand.Execute(new MpPoint(0, _resize_shortcut_nudge_amount * dir));
             },
             () => {
                 return IsHorizontalOrientation;
             });

        public ICommand WindowSizeDownCommand => new MpCommand(
             () => {
                 double dir = MainWindowOrientationType == MpMainWindowOrientationType.Bottom ? -1 : 1;
                 WindowResizeCommand.Execute(new MpPoint(0, _resize_shortcut_nudge_amount * dir));
             },
             () => {
                 return IsHorizontalOrientation;
             });

        public ICommand WindowSizeRightCommand => new MpCommand(
             () => {
                 double dir = MainWindowOrientationType == MpMainWindowOrientationType.Right ? -1 : 1;
                 WindowResizeCommand.Execute(new MpPoint(_resize_shortcut_nudge_amount * dir, 0));
             }, () => {
                 return IsVerticalOrientation;
             });

        public ICommand WindowSizeLeftCommand => new MpCommand(
             () => {
                 double dir = MainWindowOrientationType == MpMainWindowOrientationType.Right ? 1 : -1;
                 WindowResizeCommand.Execute(new MpPoint(_resize_shortcut_nudge_amount * dir, 0));
             }, () => {
                 return IsVerticalOrientation;
             });

        public ICommand WindowSizeToDefaultCommand => new MpCommand(
            () => {
                //var rc = MpAvMainView.Instance.GetResizerControl();
                //if (rc == null) {
                //    return;
                //}
                IsResizing = true;

                MpAvResizeExtension.ResetToDefault(MpAvMainView.Instance);

                IsResizing = false;
            });

        public ICommand ToggleShowMainWindowCommand => new MpCommand(() => {
            if (IsMainWindowOpen) {
                if (IsMainWindowLocked) {
                    IsMainWindowLocked = false;
                }
                HideMainWindowCommand.Execute(null);
            } else {
                ShowMainWindowCommand.Execute(null);
            }
        }, () => !IsMainWindowLoading);
        #endregion

    }
}
