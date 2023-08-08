using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Input.GestureRecognizers;
using Avalonia.Layout;
using Avalonia.Threading;
using Avalonia.VisualTree;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Avalonia;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using FocusManager = Avalonia.Input.FocusManager;

namespace MonkeyPaste.Avalonia {

    public class MpAvMainWindowViewModel :
        MpAvViewModelBase,
        MpIWindowViewModel,
        MpIIsAnimatedWindowViewModel,
        MpIActiveWindowViewModel,
        MpIWindowBoundsObserverViewModel,
        MpIWantsTopmostWindowViewModel,
        MpIResizableViewModel {
        #region Private Variables

        private double _resize_shortcut_nudge_amount = 50;
        //private CancellationTokenSource _animationCts;

        private bool _isAnimationCanceled = false;
        private DispatcherTimer _animationTimer;

        private const int _ANIMATE_WINDOW_TIMEOUT_MS = 2000;
        private const int SHOW_MAIN_WINDOW_MOUSE_HIT_ZONE_HEIGHT = 5;
        #endregion

        #region Statics

        private static MpAvMainWindowViewModel _instance;

        public static MpAvMainWindowViewModel Instance => _instance ?? (_instance = new MpAvMainWindowViewModel());
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
            IsMainWindowLocked;

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
                        //MpAvSearchCriteriaItemCollectionViewModel.Instance.BoundCriteriaListViewScreenHeight -
                        MpAvFilterMenuViewModel.Instance.FilterMenuHeight -
                        MpAvSidebarItemCollectionViewModel.Instance.ButtonGroupFixedDimensionLength;
                }
                return MainWindowHeight -
                        MpAvMainWindowTitleMenuViewModel.Instance.TitleMenuHeight -
                        //MpAvSearchCriteriaItemCollectionViewModel.Instance.BoundCriteriaListViewScreenHeight -
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
                if (!Mp.Services.PlatformInfo.IsDesktop) {
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

        public WindowState WindowState { get; set; } = WindowState.Normal;
        public bool IsMainWindowOrientationChanging { get; set; } = false;
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
            Mp.Services != null &&
            Mp.Services.PlatformInfo != null &&
            Mp.Services.PlatformInfo.IsDesktop;

        public string ShowOrHideLabel => IsMainWindowOpen ? "Hide" : "Show";
        public string ShowOrHideIconResourceKey => IsMainWindowOpen ? "ClosedEyeImage" : "OpenEyeImage";
        public bool AnimateShowWindow =>
            Mp.Services.PlatformInfo.IsDesktop &&
            MpAvPrefViewModel.Instance.AnimateMainWindow;
        public DateTime? LastDecreasedFocusLevelDateTime { get; set; }
        public bool IsAnyItemDragging {
            get {
                if (MpAvTagTrayViewModel.Instance.IsAnyDragging ||
                    MpAvTagTrayViewModel.Instance.IsAnyPinTagDragging ||
                    MpAvContentDragHelper.IsDragging ||
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

        public bool IsAnyNotificationActivating { get; set; }

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

        public MpMainWindowOrientationType MainWindowOrientationType { get; private set; }
        public MpMainWindowShowBehaviorType MainWindowShowBehaviorType { get; private set; }
        public int MainWindowMonitorIdx {
            get {
                switch (MainWindowShowBehaviorType) {
                    case MpMainWindowShowBehaviorType.Primary:
                    default:
                        // NOTE will need another monitor to build out non-primary display types
                        int monitorIdx = Mp.Services.ScreenInfoCollection.Screens.IndexOf(x => x.IsPrimary);
                        _mainWindowScreen =
                            monitorIdx < 0 ?
                            Mp.Services.ScreenInfoCollection.Screens.FirstOrDefault() :
                            Mp.Services.ScreenInfoCollection.Screens.ElementAt(monitorIdx);
                        return monitorIdx;
                }
            }
        }

        private MpIPlatformScreenInfo _mainWindowScreen;
        public MpIPlatformScreenInfo MainWindowScreen {
            get {
                // TODO mouse & active show behavior isn't implemented since it can't be tested yet
                // TODO 2 this code should be cleaned up (buggy from platform startup stuff)
                if (Mp.Services == null ||
                    Mp.Services.ScreenInfoCollection == null ||
                    Mp.Services.ScreenInfoCollection.Screens == null ||
                    !Mp.Services.ScreenInfoCollection.Screens.Any()) {
                    if (Application.Current.ApplicationLifetime is ISingleViewApplicationLifetime mobile
                        && mobile.MainView != null) {
                        return new MpAvDesktopScreenInfo(mobile.MainView.GetVisualRoot().AsScreen());
                    }
                    return new MpAvDesktopScreenInfo() { IsPrimary = true, };
                }
                if (_mainWindowScreen == null) {
                    if (MainWindowMonitorIdx < 0 &&
                        Mp.Services.ScreenInfoCollection.Screens.Any()) {

                    }
                    _mainWindowScreen = Mp.Services.ScreenInfoCollection.Screens.ElementAt(MainWindowMonitorIdx);
                    if (_mainWindowScreen == null &&
                        Application.Current.ApplicationLifetime is ISingleViewApplicationLifetime browser
                        && browser.MainView != null) {
                        _mainWindowScreen = new MpAvDesktopScreenInfo(browser.MainView.GetVisualRoot().AsScreen());
                    }
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
            MpMessenger.RegisterGlobal(ReceivedGlobalMessage);
        }

        #endregion

        #region Public Methods

        public async Task InitializeAsync() {
            MainWindowOrientationType = (MpMainWindowOrientationType)Enum.Parse(typeof(MpMainWindowOrientationType), MpAvPrefViewModel.Instance.MainWindowOrientation, false);
            MainWindowShowBehaviorType = (MpMainWindowShowBehaviorType)Enum.Parse(typeof(MpMainWindowShowBehaviorType), MpAvPrefViewModel.Instance.MainWindowShowBehaviorType, false);

            OnPropertyChanged(nameof(MainWindowScreen));
            OnPropertyChanged(nameof(IsDesktop));

            MpAvShortcutCollectionViewModel.Instance.OnGlobalMouseReleased += Instance_OnGlobalMouseReleased;
            MpAvShortcutCollectionViewModel.Instance.OnGlobalMouseMove += Instance_OnGlobalMouseMove;
            MpAvShortcutCollectionViewModel.Instance.OnGlobalMouseClicked += Instance_OnGlobalMouseClicked;
            MpAvShortcutCollectionViewModel.Instance.OnGlobalMouseWheelScroll += Instance_OnGlobalMouseWheelScroll;

            while (App.MainView == null) {
                await Task.Delay(100);
            }
            App.MainView.DataContext = this;

            MpMessenger.SendGlobal(MpMessageType.MainWindowSizeChanged);

            IsMainWindowLoading = false;

            Mp.Services.ClipboardMonitor.StartMonitor(false);

            SetupMainWindowSize();
            SetMainWindowRect(MainWindowClosedScreenRect);

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
                    if (App.MainView is not MpIMainView mv) {
                        break;
                    }
                    mv.SetPosition(MainWindowOpenedScreenRect.Location, MainWindowScreen.Scaling);

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
                    MpAvPrefViewModel.Instance.MainWindowOrientation = MainWindowOrientationType.ToString();
                    OnPropertyChanged(nameof(MainWindowLayoutOrientation));
                    break;
                case nameof(MainWindowShowBehaviorType):
                    MpAvPrefViewModel.Instance.MainWindowShowBehaviorType = MainWindowShowBehaviorType.ToString();
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
                    HideMainWindowCommand.Execute(null);
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
                        if (MpAvPrefViewModel.Instance.MainWindowInitialHeight == 0) {
                            // initial setting
                            MpAvPrefViewModel.Instance.MainWindowInitialHeight = MainWindowScreen.WorkArea.Height * MainWindowDefaultHorizontalHeightRatio;
                        }
                        MainWindowHeight = MpAvPrefViewModel.Instance.MainWindowInitialHeight;
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
                    MainWindowHeight = MainWindowScreen.WorkArea.Height;
                    if (MainWindowWidth == 0) {
                        // startup case                        
                        if (MpAvPrefViewModel.Instance.MainWindowInitialWidth == 0) {
                            // initial setting
                            MpAvPrefViewModel.Instance.MainWindowInitialWidth = MainWindowScreen.WorkArea.Width * MainWindowDefaultVerticalWidthRatio;
                        }
                        MainWindowWidth = MpAvPrefViewModel.Instance.MainWindowInitialWidth;
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
        }
        private void StartMainWindowShow() {
            SetupMainWindowSize();
            IsMainWindowOpening = true;

            if (MpAvWindowManager.MainWindow is Window w &&
                MpAvPrefViewModel.Instance.ShowInTaskbar) {
                w.WindowState = WindowState.Normal;
            }
            //if (HideInitialOpen) {
            // app started from login, initial show is transparent/nohtt
            // shows splash loader and loaded msg by default but user can hide
            if (IsMainWindowInitiallyOpening) {
                MpAvToolWindow_Win32.SetAsNoHitTestWindow(MpAvWindowManager.MainWindow.TryGetPlatformHandle().Handle);
                MpAvWindowManager.MainWindow.Opacity = 0;
                IsMainWindowInHiddenLoadState = true;
            } else if (IsMainWindowInHiddenLoadState) {
                MpAvToolWindow_Win32.RemoveNoHitTestWindow(MpAvWindowManager.MainWindow.TryGetPlatformHandle().Handle);
                MpAvWindowManager.MainWindow.Opacity = 1;
                IsMainWindowInHiddenLoadState = false;
            }
            //}
            DispatcherPriority show_priority =
                IsMainWindowInHiddenLoadState ?
                    DispatcherPriority.Background : DispatcherPriority.Normal;
            Dispatcher.UIThread.Post(App.MainView.Show, show_priority);

            IsMainWindowVisible = true;
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

            bool is_other_win_active = MpAvWindowManager.AllWindows.Any(x => x.DataContext != this && x.IsActive);

            bool force_activate =
                !IsMainWindowActive &&
                !is_other_win_active &&
                !IsMainWindowInHiddenLoadState;
            if (force_activate) {
                // when mw is shown and not active it doesn't hide or receive input until activated
                MpAvWindowManager.MainWindow.Activate();
                MpAvWindowManager.MainWindow.Topmost = true;
            }

            MpConsole.WriteLine($"SHOW WINDOW DONE. Activate Forced: '{force_activate}' Other Active: '{is_other_win_active}'");
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
            if (MpAvWindowManager.MainWindow is Window w &&
                MpAvPrefViewModel.Instance.ShowInTaskbar) {
                w.WindowState = WindowState.Minimized;
            } else {
                App.MainView.Hide();
            }
            IsMainWindowVisible = false;
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
                if (!MpAvPrefViewModel.Instance.DoShowMainWindowWithMouseEdgeAndScrollDelta) {
                    return;
                }

                bool is_core_loaded = Mp.Services != null &&
                     Mp.Services.StartupState != null &&
                     Mp.Services.StartupState.IsCoreLoaded;

                if (!IsMainWindowOpening && is_core_loaded) {
                    if (MpAvShortcutCollectionViewModel.Instance.GlobalMouseLocation != null &&
                             MpAvShortcutCollectionViewModel.Instance.GlobalMouseLocation.Y <= SHOW_MAIN_WINDOW_MOUSE_HIT_ZONE_HEIGHT) {
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
            if (!Mp.Services.StartupState.IsPlatformLoaded) {
                return;
            }
            Dispatcher.UIThread.Post(() => {
                if (IsMainWindowOpen &&
                           !IsMainWindowClosing &&
                          !IsMainWindowLocked &&
                          //!MpExternalDropBehavior.Instance.IsPreExternalTemplateDrop &&
                          MpAvShortcutCollectionViewModel.Instance.GlobalMouseLocation != null &&
                          !MainWindowScreenRect.Contains(MpAvShortcutCollectionViewModel.Instance.GlobalMouseLocation)) {
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

                if (!isShowingMainWindow &&
                    MpAvPrefViewModel.Instance.ShowMainWindowOnDragToScreenTop) {
                    if (MpAvShortcutCollectionViewModel.Instance.GlobalMouseLeftButtonDownLocation != null &&
                        gmp.Distance(MpAvShortcutCollectionViewModel.Instance.GlobalMouseLeftButtonDownLocation) >= MpAvShortcutCollectionViewModel.MIN_GLOBAL_DRAG_DIST &&
                        gmp.Y <= SHOW_MAIN_WINDOW_MOUSE_HIT_ZONE_HEIGHT) {
                        // show mw during dnd and user drags to top of screen (when pref set)
                        ShowMainWindowCommand.Execute(null);
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
                    MpAvClipTrayViewModel.Instance.IsAddingClipboardItem) {
                    await Task.Delay(100);
                    continue;
                }
                break;
            }

            await HideMainWindowCommand.ExecuteAsync();

            // only show in taskbar once initial/hidden show is complete
            MpAvWindowManager.MainWindow.Bind(
                Window.ShowInTaskbarProperty,
                new Binding() {
                    Source = MpAvPrefViewModel.Instance,
                    Path = nameof(MpAvPrefViewModel.Instance.ShowInTaskbar)
                });
            bool was_loader_visible = false;
            if (MpAvWindowManager.AllWindows.FirstOrDefault(x => x is MpAvLoaderNotificationWindow) is MpAvLoaderNotificationWindow lnw &&
                lnw.DataContext is MpAvNotificationViewModelBase nvm) {
                // only show loaded msg if progress wasn't there
                was_loader_visible = lnw.IsVisible;
                nvm.HideNotification();
            }
            // wait a bit to avoid laggy animation due to hide mw handlers
            await Task.Delay(1_000);

            MpMessenger.SendGlobal(MpMessageType.StartupComplete);
            MpAvLoaderViewModel.LoaderStopWatch.Stop();
            MpConsole.WriteLine($"Startup complete. Total Time {MpAvLoaderViewModel.LoaderStopWatch.ElapsedMilliseconds}ms");

            if (!was_loader_visible) {
                Mp.Services.NotificationBuilder.ShowMessageAsync(
                title: "Loaded",
                body: $"Monkey Paste is now loaded. \nClipboard listening is: {(MpAvClipTrayViewModel.Instance.IsAppPaused ? "Paused" : "Active")}",
                msgType: MpNotificationType.StartupComplete,
                iconSourceObj: "AppImage").FireAndForgetSafeAsync();
            }
            bool was_login_load = Mp.Services.StartupState.StartupFlags.HasFlag(MpStartupFlags.Login);
            if (!was_login_load) {
                ShowMainWindowCommand.Execute(null);
            }

        }
        #endregion

        #region Commands        

        public MpIAsyncCommand ShowMainWindowCommand => new MpAsyncCommand(
             async () => {
                 if (IsMainWindowOpening && IsMainWindowAnimating()) {
                     return;
                 }

                 await ResetMainWindowAnimationStateAsync();

                 MpConsole.WriteLine("Opening Main Window");

                 StartMainWindowShow();

                 if (AnimateShowWindow) {
                     await AnimateMainWindowAsync(MainWindowOpenedScreenRect);
                 } else {
                     SetMainWindowRect(MainWindowOpenedScreenRect);
                 }
                 FinishMainWindowShow();
             },
            () => {
                AnalyzeWindowState("show");
                bool canShow =
                    !IsMainWindowLoading &&
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

        public MpIAsyncCommand HideMainWindowCommand => new MpAsyncCommand(
            async () => {
                Dispatcher.UIThread.VerifyAccess();
                if (IsMainWindowClosing && IsMainWindowAnimating()) {
                    return;
                }

                await ResetMainWindowAnimationStateAsync();

                MpConsole.WriteLine("Closing Main Window");
                IsMainWindowClosing = true;
                //}
                if (AnimateShowWindow) {
                    await AnimateMainWindowAsync(MainWindowClosedScreenRect);
                }
                FinishMainWindowHide();
            },
            () => {
                if (Mp.Services != null &&
                    Mp.Services.PlatformInfo != null &&
                    !Mp.Services.PlatformInfo.IsDesktop) {
                    return false;
                }

                AnalyzeWindowState("hide");
                bool isInputFocused =
                    Mp.Services.FocusMonitor.FocusElement is Control c &&
                    (
                        c.GetVisualAncestor<ContextMenu>() != null ||
                        c.GetVisualAncestor<MenuItem>() != null ||
                        c.GetVisualAncestor<ComboBoxItem>() != null ||
                        (c.GetVisualAncestor<Window>() is Window w && w != MpAvWindowManager.MainWindow) ||
                        (c.GetVisualAncestor<TextBox>() is TextBox tb && !tb.IsReadOnly)
                    );

                bool isModalOpen =
                    MpAvWindowManager.AllWindows.Any(x => x.IsActive && (x.DataContext is MpIWindowViewModel && (x.DataContext as MpIWindowViewModel).WindowType == MpWindowType.Modal));
                bool canHide = !IsMainWindowLocked &&
                                  !IsAnyDropDownOpen &&
                                  !IsMainWindowInitiallyOpening &&
                                    //!IsAnyDialogOpen &&
                                    !isModalOpen &&
                                    !isInputFocused &&
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




        public ICommand ToggleShowMainWindowCommand => new MpCommand(
             () => {
                 AnalyzeWindowState("toggle");
                 bool will_open = !IsMainWindowOpen;
                 if (will_open) {
                     ShowMainWindowCommand.Execute(null);
                 } else {
                     HideMainWindowCommand.Execute(null);
                 }
             }, () => {
                 return !IsMainWindowLoading;
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
                IsMainWindowOrientationChanging = true;

                MainWindowOrientationType = (MpMainWindowOrientationType)nextOr;
                OnPropertyChanged(nameof(MainWindowTransformAngle));
                if (!Mp.Services.PlatformInfo.IsDesktop) {
                    MainWindowScreen.Rotate(MainWindowTransformAngle);
                }
                OnPropertyChanged(nameof(MainWindowScreen));
                SetupMainWindowSize(true);

                SetMainWindowRect(MainWindowOpenedScreenRect);
                //MpConsole.WriteLine($"MW Orientation: '{MainWindowOrientationType}' Angle: '{MainWindowTransformAngle}' Bounds: '{MainWindowScreen.Bounds}'");


                MpAvMainView.Instance.UpdateContentLayout();

                await Task.Delay(300);
                MpMessenger.SendGlobal(MpMessageType.MainWindowOrientationChangeEnd);
                IsMainWindowOrientationChanging = false;
            });

        public ICommand ToggleMainWindowLockCommand => new MpCommand<object>(
            (args) => {
                IsMainWindowLocked = !IsMainWindowLocked;
                if (args is ToggleButton tb) {
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
