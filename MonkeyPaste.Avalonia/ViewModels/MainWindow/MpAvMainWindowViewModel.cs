using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Layout;
using Avalonia.Threading;
using Avalonia.VisualTree;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Avalonia;
using MonkeyPaste.Common.Plugin;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MonkeyPaste.Avalonia {
    public class MpAvMainWindowViewModel :
        MpAvViewModelBase,
        MpIWindowViewModel,
        MpIIsAnimatedWindowViewModel,
        MpIActiveWindowViewModel,
        MpIWindowStateViewModel,
        MpIWindowBoundsObserverViewModel,
        MpIWantsTopmostWindowViewModel,
        MpIResizableViewModel {
        #region Private Variables

        private double _resize_shortcut_nudge_amount = 50;
        //private CancellationTokenSource _animationCts;

        private bool _isAnimationCanceled = false;
        private DispatcherTimer _animationTimer;

        private const int _ANIMATE_WINDOW_TIMEOUT_MS = 2000;
        private const int SHOW_MAIN_WINDOW_MOUSE_HIT_ZONE_DIST = 5;
        #endregion

        #region Statics

        private static MpAvMainWindowViewModel _instance;

        public static MpAvMainWindowViewModel Instance => _instance ?? (_instance = new MpAvMainWindowViewModel());

        public static bool IsPointerInTopEdgeZone() {
            return
                MpAvShortcutCollectionViewModel.Instance.GlobalScaledMouseLocation != null &&
                MpAvShortcutCollectionViewModel.Instance.GlobalScaledMouseLocation.Y <=
                    SHOW_MAIN_WINDOW_MOUSE_HIT_ZONE_DIST;
        }
        public static bool CanDragOpen() {
            var gmp = MpAvShortcutCollectionViewModel.Instance.GlobalScaledMouseLocation;
            return
                MpAvShortcutCollectionViewModel.Instance.GlobalMouseLeftButtonDownLocation != null &&
                gmp.Distance(MpAvShortcutCollectionViewModel.Instance.GlobalMouseLeftButtonDownLocation) >=
                    MpAvShortcutCollectionViewModel.MIN_GLOBAL_DRAG_DIST &&
                IsPointerInTopEdgeZone();
        }

        public static bool CanScrollOpen() {
            return IsPointerInTopEdgeZone();
        }
        #endregion

        #region Interfaces

        #region MpIActiveWindowViewModel Implementation
        bool MpIActiveWindowViewModel.IsWindowActive {
            get => IsMainWindowActive;
            set => IsMainWindowActive = value;
        }

        #endregion

        #region MpIIsAnimatedDeactiveWindowViewModel Implementation
        bool MpIIsAnimatedWindowViewModel.IsAnimated =>
            AnimateShowWindow;
        bool MpIIsAnimatedWindowViewModel.IsAnimating { get; set; }
        bool MpIIsAnimatedWindowViewModel.IsComplete =>
            !IsMainWindowAnimating();


        #endregion

        #region MpIWindowBoundsObserverViewModel Implementation
        MpRect MpIWindowBoundsObserverViewModel.Bounds {
            get => ObservedMainWindowRect;
            set => ObservedMainWindowRect = value;
        }
        MpRect MpIWindowBoundsObserverViewModel.LastBounds {
            get => LastMainWindowRect;
            set => LastMainWindowRect = value;
        }

        #endregion

        #region MpITopmostWindow Implementation
        bool MpIWantsTopmostWindowViewModel.WantsTopmost =>
            true;// IsMainWindowLocked;

        #endregion

        #region MpIWindowViewModel Implementation
        public MpWindowType WindowType =>
            MpWindowType.Main;

        #endregion

        #endregion

        #region Properties

        #region View Models

        #endregion

        #region Layout

        public double AvailableContentAndSidebarWidth {
            get {
                if (IsVerticalOrientation) {
#if MOBILE_OR_WINDOWED
                    return MainWindowWidth;
#else                    
                    return MainWindowWidth -
                        MpAvMainWindowTitleMenuViewModel.Instance.TitleMenuWidth;
#endif
                }
                return MainWindowWidth -
                        MpAvSidebarItemCollectionViewModel.Instance.ButtonGroupFixedDimensionLength;
            }
        }

        public double AvailableContentAndSidebarHeight {
            get {
                if (IsVerticalOrientation) {
#if MOBILE_OR_WINDOWED
                    var test = MpAvThemeViewModel.Instance.DefaultGridSplitterFixedDimensionLength;
                    return MainWindowHeight -
                        MpAvMainWindowTitleMenuViewModel.Instance.TitleMenuHeight -
                        MpAvFilterMenuViewModel.Instance.FilterMenuHeight -
                        //MpAvThemeViewModel.Instance.DefaultGridSplitterFixedDimensionLength -
                        (MpAvSidebarItemCollectionViewModel.Instance.SelectedItem == null ?
                            MpAvThemeViewModel.Instance.DefaultGridSplitterFixedDimensionLength : 
                            -MpAvThemeViewModel.Instance.DefaultGridSplitterFixedDimensionLength) -
                        MpAvSidebarItemCollectionViewModel.Instance.ButtonGroupFixedDimensionLength;
#else
                    return MainWindowHeight -
                        MpAvFilterMenuViewModel.Instance.FilterMenuHeight -
                        MpAvSidebarItemCollectionViewModel.Instance.ButtonGroupFixedDimensionLength;
#endif
                }
#if MOBILE_OR_WINDOWED
                return MainWindowHeight -
                        MpAvFilterMenuViewModel.Instance.FilterMenuHeight;
#else
                return MainWindowHeight -
                        MpAvMainWindowTitleMenuViewModel.Instance.TitleMenuHeight -
                        MpAvFilterMenuViewModel.Instance.FilterMenuHeight;
#endif
            }
        }

        public double MainWindowDefaultHorizontalHeightRatio =>
#if WINDOWS
            0.35;
#elif MAC
            0.45;
#elif LINUX
            0.3;
#else
            1.0d;
#endif

        public double MainWindowDefaultVerticalWidthRatio =>
#if MOBILE_OR_WINDOWED
            1.0d;
#else
            0.2;
#endif

        public double MainWindowDefaultDesiredHorizontalHeight =>
            320.0d;

        public double MainWindowDefaultDesiredVerticalWidth =>
            460.0d;

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

        public double MainWindowMinimumHorizontalHeight =>
#if MULTI_WINDOW
        200; 
#else
            0;
#endif
        public double MainWindowMinimumVerticalWidth =>
#if MULTI_WINDOW
        290; 
#else
            0;
#endif
        public double MainWindowExtentPad =>
#if MULTI_WINDOW
        20; 
#else
            0;
#endif

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
                        return MainWindowScreen.WorkingArea.Height;
                }
                return 0;
            }
        }

        public double MainWindowMaxHeight {
            get {
                switch (MainWindowOrientationType) {
                    case MpMainWindowOrientationType.Top:
                    case MpMainWindowOrientationType.Bottom:
                        return MainWindowScreen.WorkingArea.Height - MainWindowExtentPad;
                    case MpMainWindowOrientationType.Left:
                    case MpMainWindowOrientationType.Right:
                        return MainWindowScreen.WorkingArea.Height;
                }
                return 0;
            }
        }

        public double MainWindowMinWidth {
            get {
                switch (MainWindowOrientationType) {
                    case MpMainWindowOrientationType.Top:
                    case MpMainWindowOrientationType.Bottom:
                        return MainWindowScreen.WorkingArea.Width;
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
                        return MainWindowScreen.WorkingArea.Width;
                    case MpMainWindowOrientationType.Left:
                    case MpMainWindowOrientationType.Right:
                        return MainWindowScreen.WorkingArea.Width - MainWindowExtentPad;
                }
                return 0;
            }
        }

        public double MainWindowDefaultWidth {
            get {

                switch (MainWindowOrientationType) {
                    case MpMainWindowOrientationType.Top:
                    case MpMainWindowOrientationType.Bottom:
                        return MainWindowScreen.WorkingArea.Width;
                    case MpMainWindowOrientationType.Left:
                    case MpMainWindowOrientationType.Right:
#if MULTI_WINDOW
                        double max_w = MainWindowScreen.WorkingArea.Width;
                        double min_w = max_w * MainWindowDefaultVerticalWidthRatio;
                        double desired_w = MainWindowDefaultDesiredVerticalWidth;
                        return Math.Clamp(desired_w, min_w, max_w); 
#else
                        return MainWindowScreen.WorkingArea.Width;

#endif
                }
                return 0;
            }
        }

        public double MainWindowDefaultHeight {
            get {
                switch (MainWindowOrientationType) {
                    case MpMainWindowOrientationType.Top:
                    case MpMainWindowOrientationType.Bottom:
#if MULTI_WINDOW
                        double max_h = MainWindowScreen.WorkingArea.Height;
                        double min_h = max_h * MainWindowDefaultHorizontalHeightRatio;
                        double desired_h = MainWindowDefaultDesiredHorizontalHeight;
                        return Math.Clamp(desired_h, min_h, max_h); 
#else
                        return MainWindowScreen.WorkingArea.Height;
#endif
                    case MpMainWindowOrientationType.Left:
                    case MpMainWindowOrientationType.Right:
                        return MainWindowScreen.WorkingArea.Height;
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
#if MULTI_WINDOW
                switch (MainWindowOrientationType) {
                    case MpMainWindowOrientationType.Bottom:
                        return new MpRect(
                            MainWindowScreen.WorkingArea.Left,
                            MainWindowScreen.WorkingArea.Bottom - MainWindowHeight,
                            MainWindowWidth,
                            MainWindowHeight);
                    case MpMainWindowOrientationType.Top:
                        return new MpRect(
                            MainWindowScreen.WorkingArea.Left,
                            MainWindowScreen.WorkingArea.Top,
                            MainWindowWidth,
                            MainWindowHeight);
                    case MpMainWindowOrientationType.Left:
                        return new MpRect(
                            MainWindowScreen.WorkingArea.Left,
                            MainWindowScreen.WorkingArea.Top,
                            MainWindowWidth,
                            MainWindowHeight);
                    case MpMainWindowOrientationType.Right:
                        return new MpRect(
                            MainWindowScreen.WorkingArea.Right - MainWindowWidth,
                            MainWindowScreen.WorkingArea.Top,
                            MainWindowWidth,
                            MainWindowHeight);
                    default:
                        return MpRect.Empty;
                }
#else

                return MainWindowScreen.WorkingArea;
#endif
            }
        }

        public MpRect MainWindowClosedScreenRect {
            get {
                switch (MainWindowOrientationType) {
                    case MpMainWindowOrientationType.Bottom:
                        return new MpRect(
                            MainWindowScreen.WorkingArea.Left,
                            MainWindowScreen.WorkingArea.Bottom,
                            MainWindowWidth,
                            MainWindowHeight);
                    case MpMainWindowOrientationType.Top:
                        return new MpRect(
                            MainWindowScreen.WorkingArea.Left,
                            MainWindowScreen.WorkingArea.Top - MainWindowHeight,
                            MainWindowWidth,
                            MainWindowHeight);
                    case MpMainWindowOrientationType.Left:
                        return new MpRect(
                            MainWindowScreen.WorkingArea.Left - MainWindowWidth,
                            MainWindowScreen.WorkingArea.Top,
                            MainWindowWidth,
                            MainWindowHeight);
                    case MpMainWindowOrientationType.Right:
                        return new MpRect(
                            MainWindowScreen.WorkingArea.Right,
                            MainWindowScreen.WorkingArea.Top,
                            MainWindowWidth,
                            MainWindowHeight);
                }

                return new MpRect();
            }
        }

        #endregion

        #region Appearance

        public object ShowOrHideIconSourceObj =>
            new object[] {
                IsMainWindowOpen ? "ClosedEyeImage" : "OpenImage",
                IsMainWindowOpen ? MpSystemColors.red3 : MpSystemColors.navyblue };
        #endregion

        #region State

        MpIPlatformScreenInfo LastOpenedScreenInfo { get; set; }
        public WindowState WindowState { get; set; } = WindowState.Normal;
        public bool IsMainWindowOrientationChanging { get; set; } = false;
        public double MainWindowTransformAngle {
            get {
#if MULTI_WINDOW
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
            Mp.Services != null &&
            Mp.Services.PlatformInfo != null &&
            Mp.Services.PlatformInfo.IsDesktop;
        public string ShowOrHideLabel => IsMainWindowOpen ? UiStrings.MainWindowHideLabel : UiStrings.MainWindowShowLabel;
        public bool AnimateShowWindow =>
            Mp.Services.PlatformInfo.IsDesktop &&
            MpAvPrefViewModel.Instance.AnimateMainWindow;
        public DateTime? LastDecreasedFocusLevelDateTime { get; set; }
        public bool IsAnyItemDragging {
            get {
                if (MpAvTagTrayViewModel.Instance.IsAnyDragging ||
                    MpAvTagTrayViewModel.Instance.IsAnyPinTagDragging ||
                    MpAvContentWebViewDragHelper.IsDragging ||
                    IsMainWindowOrientationDragging) {
                    return true;
                }
                return false;
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
        public bool IsMainWindowInHiddenLoadState { get; private set; }

        private bool _isMainWindowLocked;
        public bool IsMainWindowLocked {
            get {
                //if (IsMainWindowSilentLocked) {
                //    return true;
                //}
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

        public bool IsAnyDropDownOpen { get; set; }

        public bool IsMainWindowActive { get; set; }

        public bool IsAnyAppWindowActive =>
            MpAvWindowManager.ActiveWindow != null;

        public bool IsFilterMenuVisible { get; set; } = true;

        public bool IsHorizontalOrientation =>
            MainWindowOrientationType == MpMainWindowOrientationType.Bottom ||
            MainWindowOrientationType == MpMainWindowOrientationType.Top;

        public bool IsVerticalOrientation =>
            !IsHorizontalOrientation;

        public bool IsLeftOrientation =>
            MainWindowOrientationType == MpMainWindowOrientationType.Left;
        public bool IsTopOrientation =>
            MainWindowOrientationType == MpMainWindowOrientationType.Top;
        public bool IsRightOrientation =>
            MainWindowOrientationType == MpMainWindowOrientationType.Right;
        public bool IsBottomOrientation =>
            MainWindowOrientationType == MpMainWindowOrientationType.Bottom;

        public Orientation MainWindowLayoutOrientation =>
            IsHorizontalOrientation ? Orientation.Horizontal : Orientation.Vertical;

        public MpMainWindowOrientationType MainWindowOrientationType {
            get => MpAvPrefViewModel.Instance.MainWindowOrientationStr.ToEnum<MpMainWindowOrientationType>();
            private set {
                if (MainWindowOrientationType != value) {
                    MpAvPrefViewModel.Instance.MainWindowOrientationStr = value.ToString();
                    OnPropertyChanged(nameof(MpMainWindowOrientationType));
                }
            }
        }
        public MpMainWindowShowBehaviorType MainWindowShowBehaviorType =>
            MpAvPrefViewModel.Instance.MainWindowShowBehaviorTypeStr.ToEnum<MpMainWindowShowBehaviorType>();
       

        private MpIPlatformScreenInfo _mainWindowScreen;
        public MpIPlatformScreenInfo MainWindowScreen {
            get {
                // TODO mouse & active show behavior isn't implemented since it can't be tested yet
                // TODO 2 this code should be cleaned up (buggy from platform startup stuff)
                if (Mp.Services == null ||
                    Mp.Services.ScreenInfoCollection == null ||
                    Mp.Services.ScreenInfoCollection.Screens == null ||
                    !Mp.Services.ScreenInfoCollection.Screens.Any()) {
                    // this may not even be needed anymore but is to avoid null screen properties before any windows create
                    if (Application.Current.ApplicationLifetime is ISingleViewApplicationLifetime mobile
                        && mobile.MainView != null) {
                        return new MpAvDesktopScreenInfo(mobile.MainView.GetVisualRoot().AsScreen());
                    } else if(Application.Current.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime lt &&
                        lt.MainWindow is { } w) {
                        return new MpAvDesktopScreenInfo(w.Screens.Primary);
                    }
                    return new MpAvDesktopScreenInfo() { IsPrimary = true, };
                }
                if (_mainWindowScreen == null) {
                    //_mainWindowScreen = Mp.Services.ScreenInfoCollection.Screens.ElementAt(MainWindowMonitorIdx);
                    _mainWindowScreen = Mp.Services.ScreenInfoCollection.Primary;
                    if (_mainWindowScreen == null &&
                        Application.Current.ApplicationLifetime is ISingleViewApplicationLifetime browser
                        && browser.MainView != null) {
                        _mainWindowScreen = new MpAvDesktopScreenInfo(browser.MainView.GetVisualRoot().AsScreen());
                    }
                }
                return _mainWindowScreen;
            }
            private set {
                if (_mainWindowScreen != value) {
                    _mainWindowScreen = value;
                    OnPropertyChanged(nameof(MainWindowScreen));
                }
            }
        }

        #endregion

        #endregion

        #region Events
        #endregion

        #region Constructors
        private MpAvMainWindowViewModel() : base() {
            if(MpAvPrefViewModel.Instance == null) {
#if MULTI_WINDOW
                MainWindowOrientationType = MpMainWindowOrientationType.Bottom;
#else
                MainWindowOrientationType = MpMainWindowOrientationType.Left;
#endif
            } else {
                MainWindowOrientationType = MpAvPrefViewModel.Instance.MainWindowOrientationStr.ToEnum<MpMainWindowOrientationType>();
            }
            PropertyChanged += MpAvMainWindowViewModel_PropertyChanged;
            MpMessenger.RegisterGlobal(ReceivedGlobalMessage);
        }

        #endregion

        #region Public Methods

        public async Task InitializeAsync() {
            OnPropertyChanged(nameof(MainWindowOrientationType));
            OnPropertyChanged(nameof(MainWindowShowBehaviorType));
            OnPropertyChanged(nameof(MainWindowScreen));
            OnPropertyChanged(nameof(IsDesktop));

            MpAvShortcutCollectionViewModel.Instance.OnGlobalMouseReleased += Instance_OnGlobalMouseReleased;
            MpAvShortcutCollectionViewModel.Instance.OnGlobalMouseMove += Instance_OnGlobalMouseMove;
            MpAvShortcutCollectionViewModel.Instance.OnGlobalMouseClicked += Instance_OnGlobalMouseClicked;
            MpAvShortcutCollectionViewModel.Instance.OnGlobalMouseWheelScroll += Instance_OnGlobalMouseWheelScroll;

#if DESKTOP
            MpAvMainView.Init(this);
            while (App.MainView == null) {
                await Task.Delay(100);
            }
            App.MainView.DataContext = this;
#else
            if (App.Instance.ApplicationLifetime is ISingleViewApplicationLifetime sval &&
                sval.MainView is Border b) {
                var test = b.Bounds;
                while (true) {
                    if (MpAvMainView.Instance != null &&
                        b.Child is Control c &&
                        c.DataContext is MpAvLoaderNotificationViewModel lnvm &&
                        lnvm.ProgressLoader is MpAvLoaderViewModel lvm &&
                        lvm.PercentLoaded > 0.5 &&
                        lvm.PendingItems.Count == 1 &&
                        lvm.PendingItems.FirstOrDefault().ItemType == typeof(MpAvMainWindowViewModel)) {
                        // wait till only mwvm is left to load 
                        break;
                    }
                    await Task.Delay(100);
                }
                MpAvMainView.Instance.DataContext = this;
                App.SetPrimaryView(MpAvMainView.Instance);
                MpAvWindowManager.AllWindows.Add(MpAvMainView.Instance);
                //sval.MainView = MpAvMainView.Instance;
                //sval.MainView.HorizontalAlignment = HorizontalAlignment.Stretch;
                //sval.MainView.VerticalAlignment = VerticalAlignment.Bottom;
                //MpAvMainView.Instance.RootGrid.VerticalAlignment = VerticalAlignment.Bottom;
            }
#endif

            MpMessenger.SendGlobal(MpMessageType.MainWindowSizeChanged);

            IsMainWindowLoading = false;

            Mp.Services.ClipboardMonitor.StartMonitor(false);

            SetupMainWindowSize();

#if MULTI_WINDOW
            SetMainWindowRect(MainWindowClosedScreenRect);
#else
            SetMainWindowRect(MainWindowOpenedScreenRect);
#endif

            ShowMainWindowCommand.Execute(null);

            // Need to delay or resizer thinks bounds are empty on initial show
            await Task.Delay(300);
            CycleOrientationCommand.Execute(null);

            MpMessenger.SendGlobal(MpMessageType.MainWindowLoadComplete);
            FinishMainWindowLoadAsync().FireAndForgetSafeAsync();
        }

        #endregion

        #region Private Methods

        private void MpAvMainWindowViewModel_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e) {
            switch (e.PropertyName) {
                case nameof(IsHovering):
                    MpConsole.WriteLine("MainWindow Hover: " + (IsHovering ? "TRUE" : "FALSE"));
                    break;
                case nameof(MainWindowHeight):
                    MpConsole.WriteLine($"Mw height: {MainWindowHeight}");
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

                case nameof(IsMainWindowVisible):
                    if (!IsMainWindowVisible) {
                        break;
                    }
#if MULTI_WINDOW
                    if (MpAvWindowManager.MainWindow == null) {
                        break;
                    }
                    MpAvWindowManager.MainWindow.Position = MainWindowOpenedScreenRect.Location.ToAvPixelPoint(MainWindowScreen.Scaling);
                    //MpConsole.WriteLine($"Vis mw position: {MpAvWindowManager.MainWindow.Position}");
#endif
                    break;
                case nameof(MainWindowOpenedScreenRect):
                    // mw is always open screen rect
                    // mw opacity mask is always open screen rect
                    // mwcg is what is animated so it hides outside current screen workarea
#if MULTI_WINDOW
                    if (MpAvWindowManager.MainWindow == null) {
                        break;
                    }
                    MpAvWindowManager.MainWindow.Position = MainWindowOpenedScreenRect.Location.ToAvPixelPoint(MainWindowScreen.Scaling);
#endif

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
                            MpAvPrefViewModel.Instance.MainWindowInitialWidth = MainWindowWidth;
                        } else {
                            MpAvPrefViewModel.Instance.MainWindowInitialHeight = MainWindowHeight;
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
                case nameof(IsMainWindowInitiallyOpening):
                    if (IsMainWindowInitiallyOpening) {
                        break;
                    }
                    MpMessenger.SendGlobal(MpMessageType.MainWindowInitialOpenComplete);
                    break;
                case nameof(IsMainWindowOpening):
                    MpMessenger.SendGlobal(MpMessageType.MainWindowOpening);
                    break;
                case nameof(IsMainWindowClosing):
                    MpMessenger.SendGlobal(MpMessageType.MainWindowClosing);
                    break;
                case nameof(MainWindowOrientationType):
                    MpAvPrefViewModel.Instance.MainWindowOrientationStr = MainWindowOrientationType.ToString();
                    OnPropertyChanged(nameof(MainWindowLayoutOrientation));
                    MpAvThemeViewModel.Instance.OnPropertyChanged(nameof(MpAvThemeViewModel.Instance.Orientation));
                    break;
                case nameof(MainWindowShowBehaviorType):
                    MpAvPrefViewModel.Instance.MainWindowShowBehaviorTypeStr = MainWindowShowBehaviorType.ToString();
                    break;
                case nameof(WindowState):
                    if (WindowState == WindowState.Minimized &&
                        IsMainWindowLocked) {
                        // can happen from Win+D so unlock to reflect state
                        IsMainWindowLocked = false;
                    }
                    break;
            }
        }
        private void ReceivedGlobalMessage(MpMessageType msg) {
            switch (msg) {
                case MpMessageType.AppWindowActivated:
                case MpMessageType.AppWindowDeactivated:
                    OnPropertyChanged(nameof(IsAnyAppWindowActive));
                    break;
                case MpMessageType.MainWindowActivated:
                    ShowMainWindowCommand.Execute(null);
                    break;
                case MpMessageType.MainWindowDeactivated:
                    HideMainWindowCommand.Execute(MpMainWindowHideType.Deactivate);
                    break;
                case MpMessageType.ScreenInfoChanged:
                    break;
            }
        }

        #region Window Animation Helpers
        private void SetupMainWindowScreen() {
            if (Mp.Services == null ||
                Mp.Services.ScreenInfoCollection is not { } sic ||
                MpAvWindowManager.MainWindow is not { } mw) {
                return;
            }
            MpIPlatformScreenInfo mw_screen = MainWindowScreen;
            bool screens_changed = sic.Refresh();
            switch (MainWindowShowBehaviorType) {
                case MpMainWindowShowBehaviorType.Primary:
                    mw_screen = sic.Primary;
                    break;
                case MpMainWindowShowBehaviorType.Mouse:
                    // NOTE need to use unscaled pointer position to locate screen since scaling is per monitor
                    if (mw.Screens.ScreenFromPoint_WORKS(MpAvShortcutCollectionViewModel.Instance.GlobalUnscaledMouseLocation) is { } pointer_screen) {
                        MpRect scaled_screen =
#if WINDOWED
                            pointer_screen.Bounds;
#else
                            pointer_screen.Bounds.ToPortableRect(pointer_screen.Scaling);
#endif
                        mw_screen = sic.Screens.FirstOrDefault(x => x.Bounds.IsEqual(scaled_screen, 1));
                    }
                    break;
            }
            bool mw_screen_changed = !mw_screen.IsEqual(MainWindowScreen);
            MainWindowScreen = mw_screen;
            if (mw_screen_changed) {
                // trigger window position change
                OnPropertyChanged(nameof(MainWindowOpenedScreenRect));
            }
            SetupMainWindowSize();
            if (screens_changed || mw_screen_changed) {
                // screen changed, update layout here
                MpAvMainView.Instance.UpdateContentLayout();
            }
        }

        private void SetupMainWindowSize(bool isOrientationChange = false) {
            switch (MainWindowOrientationType) {
                case MpMainWindowOrientationType.Top:
                case MpMainWindowOrientationType.Bottom:
                    MainWindowWidth = MainWindowScreen.WorkingArea.Width;
                    if (MainWindowHeight == 0) {
                        // startup case                        
#if MULTI_WINDOW
                        if (MpAvPrefViewModel.Instance.MainWindowInitialHeight == 0) {
                            // initial setting
                            MpAvPrefViewModel.Instance.MainWindowInitialHeight = MainWindowDefaultHeight;
                        }
                        MainWindowHeight = MpAvPrefViewModel.Instance.MainWindowInitialHeight;
#else 
                        MainWindowHeight = MainWindowDefaultHeight;
#endif
                    } else {
                        if (isOrientationChange) {
                            // clear initial width 
                            MpAvPrefViewModel.Instance.MainWindowInitialWidth = 0;
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
                    MainWindowHeight = MainWindowScreen.WorkingArea.Height;
                    if (MainWindowWidth == 0) {
                        // startup case                        
#if MULTI_WINDOW
                        if (MpAvPrefViewModel.Instance.MainWindowInitialWidth == 0) {
                            // initial setting
                            MpAvPrefViewModel.Instance.MainWindowInitialWidth = MainWindowDefaultWidth;
                        }
                        MainWindowWidth = MpAvPrefViewModel.Instance.MainWindowInitialWidth; 
#else
                        MainWindowWidth = MainWindowDefaultWidth;
#endif
                    } else {
                        if (isOrientationChange) {
                            // clear initial height
                            MpAvPrefViewModel.Instance.MainWindowInitialHeight = 0;
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

            //MainWindowWidth = MainWindowRight - MainWindowLeft;
            //MainWindowHeight = MainWindowBottom - MainWindowTop;
        }
        private void StartMainWindowShow() {
            IsMainWindowOpening = true;

#if MULTI_WINDOW
            if (MpAvWindowManager.MainWindow is not { } mw) {
                return;
            }

            if (MpAvPrefViewModel.Instance.ShowInTaskbar) {
                mw.WindowState = WindowState.Normal;
            }

            if (IsMainWindowInitiallyOpening) {
#if WINDOWS
                MpAvToolWindow_Win32.SetAsNoHitTestWindow(mw.Handle);
#endif
                // if (MpAvPrefViewModel.Instance.ShowInTaskSwitcher) {
                mw.Opacity = 0;
                //}

                IsMainWindowInHiddenLoadState = true;
            } else if (IsMainWindowInHiddenLoadState) {
#if WINDOWS
                MpAvToolWindow_Win32.RemoveNoHitTestWindow(mw.Handle);
#endif
                mw.Opacity = 1;
                IsMainWindowInHiddenLoadState = false;
            }

            Dispatcher.UIThread.Post(() => mw.Show(null));

            // BUG WindowState=Minimized/Normal makes mw ignore topmost order
            // what does work is always using window.Hide() (instead of minimize)
            // but .hide() hides mw from everywhere, taskbar, taskswitch etc.

            // below fixes WindowState issue 
            // reactivate any other visible windows
            MpAvWindowManager.TopmostWindowsByZOrder
                .Where(x => x is not MpAvMainWindow)
                .OrderBy(x => x.LastActiveDateTime)
                .ForEach(x => x.Activate());
#endif

            IsMainWindowVisible = true;
            SetupMainWindowScreen();
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
            LastOpenedScreenInfo = MainWindowScreen;

            //MpConsole.WriteLine($"SHOW WINDOW DONE. MW Orientation: '{MainWindowOrientationType}' Angle: '{MainWindowTransformAngle}' Bounds: '{MainWindowScreen.Bounds}'");
        }
        public void FinishMainWindowHide() {

            if (_isAnimationCanceled) {
                MpConsole.WriteLine("FinishHide canceled, ignoring view changes");
                return;
            }

            IsMainWindowLocked = false;
            IsMainWindowOpen = false;
            IsMainWindowClosing = false;

            SetMainWindowRect(MainWindowClosedScreenRect);

            if (MpAvPrefViewModel.Instance.ShowInTaskbar) {
                WindowState = WindowState.Minimized;
            } else if (MpAvWindowManager.MainWindow is MpAvWindow w) {
                w.Hide();
            }
            IsMainWindowVisible = false;
            //MpConsole.WriteLine("CLOSE WINDOW DONE");
        }
        private async Task AnimateMainWindowAsync(MpRect endRect) {
            // close 0.12 20
            // open 
            double zeta = 0.22d;
            double omega = 25;
            double[] cur_edges = new double[] { MainWindowLeft, MainWindowTop, MainWindowRight, MainWindowBottom };
            double[] end_edges = endRect.Sides;
            double[] edge_vels = new double[4];
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
                for (int i = 0; i < cur_edges.Length; i++) {
                    if (i == anchor_idx) {
                        // anchor_idx is 'critically dampened' to 1 so it does not oscillate (doesn't animate past screen edge)
                        MpAnimationHelpers.Spring(ref cur_edges[i], ref edge_vels[i], end_edges[i], dt, 1, omega);
                    } else {
                        MpAnimationHelpers.Spring(ref cur_edges[i], ref edge_vels[i], end_edges[i], dt, zeta, omega);
                    }
                }
                bool is_v_zero = edge_vels.All(x => Math.Abs(x) <= min_done_v);

                if (is_v_zero || _isAnimationCanceled) {
                    // consider done when all v's are pretty low or canceled
                    isDone = true;
                    _animationTimer.Stop();
                    return;
                }
                SetMainWindowRect(new MpRect(cur_edges));
            };

            _animationTimer.Tick += tick;
            _animationTimer.Start();

            var timeout_sw = Stopwatch.StartNew();
            while (!isDone) {
                await Task.Delay(5);
                if (_isAnimationCanceled) {
                    break;
                }
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
            if (!Dispatcher.UIThread.CheckAccess()) {
                Dispatcher.UIThread.Post(() => Instance_OnGlobalMouseWheelScroll(sender, delta));
                return;
            }

            bool has_scroll_gesture =
                MpAvPrefViewModel.Instance.ScrollToOpen ||
                MpAvPrefViewModel.Instance.ScrollToOpenAndLockType != MpScrollToOpenAndLockType.None;

            if (!has_scroll_gesture) {
                return;
            }

            bool is_ready = Mp.Services != null &&
                 Mp.Services.StartupState != null &&
                 Mp.Services.StartupState.IsReady;

            if (!IsMainWindowOpening && is_ready) {
                if (CanScrollOpen()) {
                    var gmp = MpAvShortcutCollectionViewModel.Instance.GlobalScaledMouseLocation;
                    bool lock_on_open = false;
                    switch (MpAvPrefViewModel.Instance.ScrollToOpenAndLockType) {
                        case MpScrollToOpenAndLockType.Always:
                            lock_on_open = true;
                            break;
                        case MpScrollToOpenAndLockType.TopLeftCorner:
                            lock_on_open = gmp.X <= SHOW_MAIN_WINDOW_MOUSE_HIT_ZONE_DIST;
                            break;
                        case MpScrollToOpenAndLockType.TopRightCorner:
                            if (MpAvWindowManager.Screens is { } scrs) {
                                // double max_right = Mp.Services.ScreenInfoCollection.Screens.Max(x => x.Bounds.Right);
                                double max_right = scrs.All.Max(x => x.Bounds.Right / x.Scaling);
                                lock_on_open = max_right - gmp.X <= SHOW_MAIN_WINDOW_MOUSE_HIT_ZONE_DIST;
                            }
                            break;
                        case MpScrollToOpenAndLockType.HoldingShift:
                            lock_on_open = MpAvShortcutCollectionViewModel.Instance.GlobalIsShiftDown;
                            break;
                    }
                    if (!lock_on_open && !MpAvPrefViewModel.Instance.ScrollToOpen) {
                        // no lock gesture detected and scroll to open disabled so ignore
                        return;
                    }
                    var gump = MpAvShortcutCollectionViewModel.Instance.GlobalUnscaledMouseLocation;
                    // get screen by sca
                    var av_scroll_screen = MpAvWindowManager.Screens.All.FirstOrDefault(x => x.Bounds.Contains(gump));
                    if (av_scroll_screen == null) {
                        return;
                    }
                    Mp.Services.ScreenInfoCollection.Refresh();
                    MpIPlatformScreenInfo scroll_screen =
                        Mp.Services.ScreenInfoCollection
                        .Screens
                        .OfType<MpAvScreenInfoBase>()
                        .FirstOrDefault(x => x.IsEqual(av_scroll_screen));
                    MpDebug.Assert(scroll_screen != null, $"Screen conv error from av screen '{av_scroll_screen}'");
                    // show mw on top edge scroll flick
                    ShowMainWindowCommand.Execute(new object[] { lock_on_open, scroll_screen });
                }
            }
        }

        private void Instance_OnGlobalMouseClicked(object sender, bool isLeftButton) {
            if (!Mp.Services.StartupState.IsPlatformLoaded) {
                return;
            }
            Dispatcher.UIThread.Post(() => {
                HideMainWindowCommand.Execute(MpMainWindowHideType.Click);
            });

        }

        private void Instance_OnGlobalMouseReleased(object sender, bool isLeftButton) {
            if (!Mp.Services.StartupState.IsPlatformLoaded) {
                return;
            }
            Dispatcher.UIThread.Post(() => {

                HideMainWindowCommand.Execute(MpMainWindowHideType.Click);
            });
        }
        private void Instance_OnGlobalMouseMove(object sender, MpPoint gmp) {
            Dispatcher.UIThread.Post(() => {
                if (IsMainWindowOpen) {
                    return;
                }
                bool isShowingMainWindow = false;

                if (!isShowingMainWindow &&
                    MpAvPrefViewModel.Instance.DragToOpen) {

                    if (CanDragOpen()) {
                        // show mw during dnd and user drags to top of screen (when pref set)
                        // NOTE passing false to signify its a show from gesture
                        ShowMainWindowCommand.Execute(false);
                    }
                }
            });

        }

        #endregion

        private void AnalyzeWindowState(string source) {
            //return;
            // this fixes:
            // 1. mw is locked so show/hide won't execute but is either NOT visible
            //    or NOT active (which will still report visible on windows)
            //    so only way to show is activate from task bar

            if (source == "show" && IsMainWindowOpen && IsMainWindowLocked) {
                // CAse 1
                bool can_activate = !IsMainWindowInHiddenLoadState;
                bool needs_activate = !IsMainWindowVisible || !IsMainWindowActive;
                if (can_activate && needs_activate) {
                    //if(source == "show") {
                    //MpConsole.WriteLine($"Fixing mw state, locked but not visible");
                    MpAvWindowManager.MainWindow.Activate();
                    //}                    
                } else {
                    if (source == "show") {

                        //MpConsole.WriteLine($"Fixing mw state, to allow show");
                        //IsMainWindowOpen = false;
                    } else {
                        //MpConsole.WriteLine($"Fixing mw state, open but not visible, finishing hide...");
                        // FinishMainWindowHide();
                    }

                }
            }
        }

        private async Task FinishMainWindowLoadAsync() {
            // wait for mw to come into view..
            while (IsMainWindowInitiallyOpening) {
                await Task.Delay(100);
            }
            await Task.Delay(300);
            // wait for for any busys
            var wait_vml = new MpIAsyncCollectionObject[] {
                MpAvClipTrayViewModel.Instance,
                MpAvTagTrayViewModel.Instance,
                MpAvAnalyticItemCollectionViewModel.Instance,
                MpAvTriggerCollectionViewModel.Instance,
                MpAvClipboardHandlerCollectionViewModel.Instance,
                MpAvPlainHtmlConverter.Instance
            };
            while (true) {
                if (wait_vml.Any(x => x.IsAnyBusy) ||
                    //!MpAvPlainHtmlConverter.Instance.IsLoaded ||
                    !Mp.Services.StartupState.IsPlatformLoaded ||
                    MpAvClipTrayViewModel.Instance.IsAddingClipboardItem) {
                    await Task.Delay(100);
                    continue;
                }
                break;
            }

#if MULTI_WINDOW
            await HideMainWindowCommand.ExecuteAsync(MpMainWindowHideType.Force);

            // only show in taskbar once initial/hidden show is complete
            MpAvWindowManager.MainWindow.Bind(
                Window.ShowInTaskbarProperty,
                new Binding() {
                    Source = MpAvPrefViewModel.Instance,
                    Path = nameof(MpAvPrefViewModel.Instance.ShowInTaskbar)
                });
            bool was_loader_visible = false;
            if (MpAvWindowManager.AllWindows.FirstOrDefault(x => x.DataContext is MpAvLoaderNotificationViewModel) is { } lnw &&
                lnw.DataContext is MpAvNotificationViewModelBase nvm) {
                // only show loaded msg if progress wasn't there
                was_loader_visible = lnw.IsVisible;
                nvm.HideNotification();
            }
            // wait a bit to avoid laggy animation due to hide mw handlers
            await Task.Delay(1_000);

            if (!was_loader_visible) {
                Mp.Services.NotificationBuilder.ShowMessageAsync(
                title: UiStrings.MainWindowLoadedNtfTitle,
                body: string.Format(UiStrings.MainWindowLoadedNtfText, MpAvClipTrayViewModel.Instance.IsIgnoringClipboardChanges ? UiStrings.CommonPausedLabel : UiStrings.CommonActiveLabel),
                msgType: MpNotificationType.StartupComplete,
                iconSourceObj: "AppImage").FireAndForgetSafeAsync();
            }
            bool was_login_load = Mp.Services.StartupState.IsLoginLoad;
            if (!was_login_load) {
                ShowMainWindowCommand.Execute(null);
            }
#endif

            MpMessenger.SendGlobal(MpMessageType.StartupComplete);
            MpAvLoaderViewModel.LoaderStopWatch.Stop();
            MpConsole.WriteLine($"Startup complete. Total Time {MpAvLoaderViewModel.LoaderStopWatch.ElapsedMilliseconds}ms");


        }
#endregion

        #region Commands        

        public MpIAsyncCommand<object> ShowMainWindowCommand => new MpAsyncCommand<object>(
             async (args) => {
                 if (IsMainWindowOpening && IsMainWindowAnimating()) {
                     return;
                 }

                 await ResetMainWindowAnimationStateAsync();

                 //MpConsole.WriteLine("Opening Main Window");

                 StartMainWindowShow();

                 if (AnimateShowWindow) {
                     await AnimateMainWindowAsync(MainWindowOpenedScreenRect);
                 } else {
                     SetMainWindowRect(MainWindowOpenedScreenRect);
                 }
                 FinishMainWindowShow();

                 if (args is object[] argParts && argParts[0] is bool lock_on_open && lock_on_open) {
                     IsMainWindowLocked = true;
                 }
             },
            (args) => {
                AnalyzeWindowState("show");
                bool has_screen_changed = false;
                if (args is object[] argParts &&
                    argParts[1] is MpIPlatformScreenInfo show_screen) {
                    has_screen_changed = !MainWindowScreen.IsEqual(show_screen);
                    if (has_screen_changed) {

                    }
                }
                bool canShow =
                    //!IsAnyDialogOpen &&
                    //!IsMainWindowClosing &&
                    !IsMainWindowLoading &&
                    (!IsMainWindowOpen || has_screen_changed) &&
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

        public MpIAsyncCommand ForceMinimizeMainWindowCommand => new MpAsyncCommand(
            async () => {
                if (IsMainWindowLocked) {
                    ToggleMainWindowLockCommand.Execute(null);
                }
                await HideMainWindowCommand.ExecuteAsync(MpMainWindowHideType.Force);
            },
            () => {
                return IsMainWindowOpen;
            });

        private bool CanHideMainWindow(object args) {
#if MOBILE_OR_WINDOWED

            return false;
#else
            MpMainWindowHideType hide_type = args == null ? MpMainWindowHideType.None : (MpMainWindowHideType)args;
            if (hide_type == MpMainWindowHideType.Force) {
                return true;
            }

            switch (hide_type) {
                case MpMainWindowHideType.Force:
                    return true;
                case MpMainWindowHideType.Deactivate:
                    return false;
                case MpMainWindowHideType.Click:
                    var gmp = MpAvShortcutCollectionViewModel.Instance.GlobalScaledMouseLocation;
                    if (MpAvShortcutCollectionViewModel.Instance.IsGlobalHooksPaused ||
                        gmp == null) {
                        return false;
                    }
                    bool isInputFocused =
               Mp.Services.FocusMonitor.FocusElement is Control c &&
               (
                   c.GetVisualAncestor<ContextMenu>() != null ||
                   c.GetVisualAncestor<MenuItem>() != null ||
                   c.GetVisualAncestor<ComboBoxItem>() != null ||
                   (c.GetVisualAncestor<MpAvWindow>() is MpAvWindow w && w != MpAvWindowManager.MainWindow) ||
                   (c.GetVisualAncestor<TextBox>() is TextBox tb && !tb.IsReadOnly)
               );

                    bool isModalActive =
                        MpAvWindowManager.AllWindows
                        .Any(x => x.Owner == MpAvWindowManager.MainWindow && x.WindowType == MpWindowType.Modal);

                    bool is_any_other_opening_or_closing =
                        MpAvWindowManager.IsAnyChildWindowOpening || MpAvWindowManager.IsAnyChildWindowClosing;

                    bool isNtfActive =
                        MpAvWindowManager.AllWindows.Any(x => x.IsActive && x.DataContext is MpAvNotificationViewModelBase);

                    bool is_click_off =
                        !MainWindowScreenRect.Contains(gmp);

                    bool canHide =
                        is_click_off &&
                        IsMainWindowOpen &&
                        !MpAvWindowManager.IsAnyActive &&
                        !IsMainWindowClosing &&
                        !IsMainWindowLocked &&
                        !IsAnyDropDownOpen &&
                        !IsMainWindowInitiallyOpening &&
                        !is_any_other_opening_or_closing &&
                        !isModalActive &&
                        !isInputFocused &&
                        !IsAnyItemDragging &&
                        !isNtfActive &&
                        !MpAvShortcutCollectionViewModel.Instance.GlobalIsMouseLeftButtonDown && // reject drag cancel event
                        !IsResizing;
#if MAC && false
                    if (!canHide) {
                        MpConsole.WriteLine($"is_click_off {is_click_off}");
                        MpConsole.WriteLine($"IsMainWindowOpen {IsMainWindowOpen}");
                        MpConsole.WriteLine($"!MpAvWindowManager.IsAnyActive {!MpAvWindowManager.IsAnyActive}");
                        MpConsole.WriteLine($"!IsMainWindowClosing {!IsMainWindowClosing}");
                        MpConsole.WriteLine($"!IsMainWindowLocked {!IsMainWindowLocked}");
                        MpConsole.WriteLine($"!IsAnyDropDownOpen {!IsAnyDropDownOpen}");
                        MpConsole.WriteLine($"!IsMainWindowInitiallyOpening {!IsMainWindowInitiallyOpening}");
                        MpConsole.WriteLine($"!is_any_other_opening_or_closing {!is_any_other_opening_or_closing}");
                        MpConsole.WriteLine($"!isModalActive {!isModalActive}");
                        MpConsole.WriteLine($"!isInputFocused {!isInputFocused}");
                        MpConsole.WriteLine($"!IsAnyItemDragging {!IsAnyItemDragging}");
                        MpConsole.WriteLine($"!isNtfActive {!isNtfActive}");
                        MpConsole.WriteLine($"!MpAvShortcutCollectionViewModel.Instance.GlobalIsMouseLeftButtonDown {!MpAvShortcutCollectionViewModel.Instance.GlobalIsMouseLeftButtonDown}");
                        MpConsole.WriteLine($"!IsResizing {!IsResizing}");
                    } 
#endif
                    return canHide;
                default:
                    return false;
            }
#endif

        }
        public MpIAsyncCommand<object> HideMainWindowCommand => new MpAsyncCommand<object>(
            async (args) => {
                Dispatcher.UIThread.VerifyAccess();
                if (IsMainWindowClosing && IsMainWindowAnimating()) {
                    return;
                }

                await ResetMainWindowAnimationStateAsync();

                //MpConsole.WriteLine("Closing Main Window");
                IsMainWindowClosing = true;
                //}
                if (AnimateShowWindow) {
                    await AnimateMainWindowAsync(MainWindowClosedScreenRect);
                }
                FinishMainWindowHide();
            },
            (args) => {
                return CanHideMainWindow(args);
            });

        public ICommand ToggleShowMainWindowCommand => new MpCommand(
             () => {
                 AnalyzeWindowState("toggle");
                 bool will_open = !(IsMainWindowOpen || IsMainWindowOpening) || IsMainWindowClosing;
                 if (will_open) {
                     ShowMainWindowCommand.Execute(null);
                 } else {
                     ForceMinimizeMainWindowCommand.Execute(null);
                 }
             }, () => {
                 return !IsMainWindowLoading;
             });
        public MpIAsyncCommand OrientWindowLeftCommand => new MpAsyncCommand(
            async () => {
                await CycleOrientationCommand.ExecuteAsync(MpMainWindowOrientationType.Left);
            });
        public MpIAsyncCommand OrientWindowRightCommand => new MpAsyncCommand(
            async () => {
                await CycleOrientationCommand.ExecuteAsync(MpMainWindowOrientationType.Right);
            });
        public MpIAsyncCommand OrientWindowTopCommand => new MpAsyncCommand(
            async () => {
                await CycleOrientationCommand.ExecuteAsync(MpMainWindowOrientationType.Top);
            });
        public MpIAsyncCommand OrientWindowBottomCommand => new MpAsyncCommand(
            async () => {
                await CycleOrientationCommand.ExecuteAsync(MpMainWindowOrientationType.Bottom);
            });

        public MpIAsyncCommand<object> CycleOrientationCommand => new MpAsyncCommand<object>(
            async (dirStrOrEnumArg) => {
                while (MpAvMainView.Instance == null) {
                    await Task.Delay(100);
                }

                int nextOr = (int)MainWindowOrientationType;

                if (dirStrOrEnumArg is string dirStr) {
                    bool isCw = dirStr.ToLowerInvariant() == "cw";
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
                IsMainWindowOrientationChanging = true;

                bool was_horiz = IsHorizontalOrientation;
                MainWindowOrientationType = (MpMainWindowOrientationType)nextOr;

                bool did_change = IsHorizontalOrientation != was_horiz;
                OnPropertyChanged(nameof(MainWindowTransformAngle));
                if (!MpAvThemeViewModel.Instance.IsMultiWindow && did_change) {
                    MainWindowScreen.Rotate(MainWindowTransformAngle);
#if WINDOWED
                    if (TopLevel.GetTopLevel(MpAvMainView.Instance) is Window w) {
                        //w.CanResize = true;
                        //var new_bounds = new MpRect(0, 0, w.Bounds.Height, w.Bounds.Width);
                        //w.Width = new_bounds.Width;
                        //w.Height = new_bounds.Height;
                        //w.CanResize = false;
                        //w.CanResize = true;
                        if(IsHorizontalOrientation) {
                            w.Width = 740;
                            w.Height = 360;
                        } else {
                            w.Width = 360;
                            w.Height = 740;
                        }
                        if(w.Content is MpAvWindow cw) {
                            cw.Width = w.Width;
                            cw.Height = w.Height;
                        }
                        _mainWindowScreen = null;
                        Mp.Services.ScreenInfoCollection = new MpAvDesktopScreenInfoCollection(w);
                        MpMessenger.SendGlobal(MpMessageType.ScreenInfoChanged);
                    }
#endif
                }
                OnPropertyChanged(nameof(MainWindowScreen));
                SetupMainWindowSize(true);

                SetMainWindowRect(MainWindowOpenedScreenRect);
                MpConsole.WriteLine($"MW Orientation: '{MainWindowOrientationType}' Angle: '{MainWindowTransformAngle}' Bounds: '{MainWindowScreen.Bounds}'");

                // first pass adjusts grid definitions, most measuring dimensions aren't updated yet
                MpAvMainView.Instance.UpdateContentLayout();

                MpAvThemeViewModel.Instance.OnPropertyChanged(nameof(MpAvThemeViewModel.Instance.Orientation));

                await Task.Delay(300);
                MpMessenger.SendGlobal(MpMessageType.MainWindowOrientationChangeEnd);
                // second pass finishes change with the right measurements
                MpAvMainView.Instance.UpdateContentLayout();
                IsMainWindowOrientationChanging = false;
            });

        public ICommand ToggleMainWindowLockCommand => new MpCommand<object>(
            (args) => {
                bool new_state = !IsMainWindowLocked;
                bool last_state = IsMainWindowLocked;
                IsMainWindowLocked = !IsMainWindowLocked;
                if (args is ToggleButton tb) {
                    if (IsMainWindowLocked == last_state) {
                        // state didn't change, silent lock overriding get
                        IsMainWindowSilentLocked = new_state;
                        IsMainWindowLocked = new_state;
                        MpDebug.Assert(IsMainWindowLocked == new_state, $"Mw lock state error");
                    }
                    tb.IsChecked = IsMainWindowLocked;
                }
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

        public ICommand WindowSizeToDefaultCommand => new MpAsyncCommand(
            async () => {
                IsResizing = true;
                IsMainWindowSilentLocked = true;

                MpAvResizeExtension.ResetToDefault(MpAvMainView.Instance);

                IsResizing = false;

                // NOTE adding signif delay to ignore off window click during resize
                // since event is triggered on double DOWN and OS (windows) still treats up as task change
                await Task.Delay(1000);
                IsMainWindowSilentLocked = false;
            });

        #endregion

    }
}
