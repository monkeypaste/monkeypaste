using Avalonia;
using Avalonia.Controls;
using Avalonia.Threading;
using Avalonia.VisualTree;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Avalonia;
using MonkeyPaste.Common.Plugin;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

#if WINDOWS
using MonkeyPaste.Common.Wpf;
#endif

namespace MonkeyPaste.Avalonia {

    public interface MpIWindowStateViewModel : MpIWindowViewModel {
        WindowState WindowState { get; set; }
    }

    public static class MpAvWindowManager {
        #region Private Variables
        private static Dictionary<MpAvWindow, IEnumerable<IDisposable>> _dispLookup = new Dictionary<MpAvWindow, IEnumerable<IDisposable>>();
        private static Dictionary<MpAvWindow, WindowState> _restoreStateLookup = new Dictionary<MpAvWindow, WindowState>();
        private static List<MpAvWindow> _waitingToClose = new List<MpAvWindow>();
        #endregion

        #region Properties
        public static List<MpAvWindow> OpeningWindows { get; set; } = [];
        public static Screens Screens =>
            AllWindows.Any() ? AllWindows.FirstOrDefault().Screens : null;
        public static ObservableCollection<MpAvWindow> AllWindows { get; private set; } = new ObservableCollection<MpAvWindow>();
        public static IReadOnlyList<MpAvWindow> TopmostWindowsByZOrder =>
#if MAC
        AllWindows
                .Where(x => x.WantsTopmost && x.WindowState != WindowState.Minimized)
                .OrderBy(x => (int)x.BindingContext.WindowType)
                .ToList();
#else
        AllWindows
                .Where(x => x.Owner == null && x.WantsTopmost && x.WindowState != WindowState.Minimized)
                .OrderBy(x => (int)x.BindingContext.WindowType)
                .ToList(); 
#endif

        public static bool IsAnyChildWindowOpening =>
            OpeningWindows.Where(x => x is not MpAvMainWindow).Any();
        public static bool IsAnyChildWindowClosing =>
            _waitingToClose.Where(x => x is not MpAvMainWindow).Any();

        public static MpAvWindow MainWindow =>
            AllWindows.FirstOrDefault(x => x is MpAvMainWindow);

        public static MpAvWindow ActiveWindow =>
#if LINUX
        AllWindows.FirstOrDefault(x => x.IsActive);
#else
        AllWindows.FirstOrDefault(x => x.IsActive); 
#endif
        public static MpAvWindow LastActiveWindow =>
            AllWindows.OrderByDescending(x => x.LastActiveDateTime).FirstOrDefault();

        public static MpAvWindow CurrentOwningWindow =>
            AllWindows
            .Where(x => x.DataContext is not MpAvNotificationViewModelBase && x.WindowState != WindowState.Minimized && x.IsVisible)
            .OrderByDescending(x => x.LastActiveDateTime)
            .FirstOrDefault();

        public static IReadOnlyList<MpAvNotificationWindow> Notifications =>
            AllWindows.OfType<MpAvNotificationWindow>().ToList();

        public static IReadOnlyList<MpAvWindow> ToastNotifications =>
            AllWindows.Where(x => x.Classes.Contains("toast")).ToList();

        public static bool IsAnyActive =>
            ActiveWindow != null;

        public static nint PrimaryHandle {
            get {
                MpAvWindow w = null;
                if (MainWindow != null) {
                    w = MainWindow;
                } else if (AllWindows.OrderBy(x => (int)x.WindowType).FirstOrDefault() is MpAvWindow ow) {
                    w = ow;
                }
                if (w == null) {
                    return IntPtr.Zero;
                }
                return w.TryGetPlatformHandle().Handle;
            }
        }

        #endregion

        #region Events
        #endregion

        static MpAvWindowManager() {
            AllWindows.CollectionChanged += AllWindows_CollectionChanged;
        }

        #region Public Methods

        public static object FindByHashCode(object codeObj) {
            int code = 0;
            if (codeObj is string codeStr) {
                if (codeStr.StartsWith("#") &&
                        codeStr.Replace("#", string.Empty) is string hashStr) {
                    codeStr = hashStr;
                }
                code = int.Parse(codeStr);
            } else if (codeObj is int) {
                code = (int)codeObj;
            }
            if (code == 0) {
                return null;
            }
            foreach (var w in AllWindows) {
                var result = w.FindVisualDescendantWithHashCode(code, true);
                if (result != null) {
                    return result;
                }
            }
            foreach (var w in AllWindows) {
                var result = w.FindLogicalDescendantWithHashCode(code, true);
                if (result != null) {
                    return result;
                }
            }
            MpConsole.WriteLine($"No visuals found for hash code: '{code}'");
            return null;
        }

        public static void CloseAll() {
            var wc = AllWindows.Count;
            for (int i = 0; i < wc; i++) {
                if (AllWindows.Any()) {
                    AllWindows[0].Close();
                } else {
                    return;
                }
            }
        }
        public static MpAvWindow LocateWindow(MpPoint gmp) {
            return AllWindows.FirstOrDefault(x => x.ScaledScreenRect().Contains(gmp));
        }
        public static MpAvWindow LocateWindow(object dataContext, bool scanDescendants = false) {
            if (!Dispatcher.UIThread.CheckAccess()) {
                var result = Dispatcher.UIThread.Invoke(() => LocateWindow(dataContext));
                return result;
            }
            if (AllWindows.FirstOrDefault(x => x.DataContext == dataContext) is MpAvWindow w) {
                return w;
            }
            if (!scanDescendants) {
                return null;
            }
            // search whole tree
            foreach (var cw in AllWindows) {
                if (cw.GetVisualDescendants<Control>().Any(x => x.DataContext == dataContext)) {
                    return cw;
                }
            }
            return null;
        }
        public static Visual LocateVisual<T>(object dataContext) where T : Visual {
            if (!Dispatcher.UIThread.CheckAccess()) {
                var result = Dispatcher.UIThread.Invoke(() => LocateWindow(dataContext));
                return result;
            }
            return
                AllWindows
                .SelectMany(x => x.GetVisualDescendants<T>())
                .FirstOrDefault(x => x.DataContext == dataContext);
        }

        public static string ToWindowTitleText(this string title) {
            string prefix = string.IsNullOrEmpty(title) ? string.Empty : $"{title} - ";
            return $"{prefix}{Mp.Services.ThisAppInfo.ThisAppProductName}";
        }

        public static Visual GetTopLevel(this Visual visual, bool logical = false) {
            if(visual == null) {
                return null;
            }
            var actual_tl = TopLevel.GetTopLevel(visual);
            if (!logical) {
                return actual_tl;
            }
            if(visual.GetVisualAncestor<MpAvWindow>() is { } w) {
                return w;
            }
            return actual_tl;
        }

        #endregion

        #region Private Methods

        private static void AllWindows_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e) {
            if (e.NewItems != null) {
                foreach (MpAvWindow nw in e.NewItems) {
                    AttachAllHandlers(nw);
                }
            }
            if (e.OldItems != null) {
                foreach (MpAvWindow ow in e.OldItems) {
                    DetachAllHandlers(ow);
                }
            }
        }
        private static void BoundsChangedHandler(MpAvWindow w, AvaloniaPropertyChangedEventArgs<Rect> e) {
            if (w.DataContext is MpIWindowBoundsObserverViewModel wbovm) {
                var oldAndNewVals = e.GetOldAndNewValue<Rect>();
                wbovm.LastBounds = oldAndNewVals.oldValue.ToPortableRect();
                wbovm.Bounds = oldAndNewVals.newValue.ToPortableRect();
            }
        }

        private static void TopmostChangedHandler(MpAvWindow w, AvaloniaPropertyChangedEventArgs<bool> e) {
            UpdateTopmost();
        }


        private static void IsVisibleChangedHandler(MpAvWindow w, AvaloniaPropertyChangedEventArgs<bool> e) {
            UpdateTopmost();
        }

        private static void WindowStateChangedHandler(MpAvWindow w, AvaloniaPropertyChangedEventArgs<bool> e) {
            if (w.DataContext is MpIWindowStateViewModel wsvm) {
                wsvm.WindowState = w.WindowState;
            }
            if (w.DataContext is MpIActiveWindowViewModel rwvm) {
                rwvm.IsWindowActive = w.WindowState != WindowState.Minimized;
            }
            if (w.WindowState != WindowState.Minimized) {
                _restoreStateLookup.AddOrReplace(w, w.WindowState);
            }
        }
        private static void Window_DataContextChanged(object sender, EventArgs e) {
            if (sender is not MpAvWindow w) {
                return;
            }

            AttachWindowViewModelHandlers(w);
        }

        private static void Window_Activated(object sender, EventArgs e) {
            if (sender is not MpAvWindow w) {
                return;
            }
            w.LastActiveDateTime = DateTime.Now;
            MpMessenger.SendGlobal(MpMessageType.AppWindowActivated);

            if (w.DataContext is MpIActiveWindowViewModel awvm) {
                awvm.IsWindowActive = true;
            }
            if (w.DataContext is MpIIsAnimatedWindowViewModel adwvm &&
                adwvm.IsAnimated && !adwvm.IsComplete && !adwvm.IsAnimating) {
                //w.Topmost = true;
                ShowWhileAnimatingAsync(w).FireAndForgetSafeAsync();
            } else {
                UpdateTopmost();
            }
        }
        private static void Window_Deactivated(object sender, EventArgs e) {
            if (sender is not MpAvWindow w) {
                return;
            }
            MpMessenger.SendGlobal(MpMessageType.AppWindowDeactivated);

            if (w.DataContext is MpIActiveWindowViewModel awvm) {
                awvm.IsWindowActive = false;
            }
            if (w.DataContext is MpIIsAnimatedWindowViewModel adwvm &&
                adwvm.IsAnimated && !adwvm.IsComplete && !adwvm.IsAnimating) {
                ShowWhileAnimatingAsync(w).FireAndForgetSafeAsync();
            } else {
                UpdateTopmost();
                //StartChildLifecycleChangeDelay(w);
            }
        }

        private static void Window_Closing(object sender, WindowClosingEventArgs e) {
            if (sender is not MpAvWindow w) {
                return;
            }

            if (w.DataContext is MpIWindowHandlesClosingViewModel whcvm &&
                    whcvm.IsWindowCloseHandled) {
                // without this check closing evt is called twice in impl 
                // handler from extra w.Close below
                return;
            }
            if (!_waitingToClose.Contains(w)) {
                _waitingToClose.Add(w);
            }

            // workaround for https://github.com/AvaloniaUI/Avalonia/pull/10951
            w.GetVisualDescendants()
                .Where(x => x is MpIOverrideRender)
                .Cast<MpIOverrideRender>()
                .ForEach(x => x.IgnoreRender = true);

            if (w.DataContext is MpIDisposableObject disp_obj) {

                // NOTE used to dispose webview and cancel js
                //disp_obj.Dispose();
                //w.DataContext = null;
            }
            if (w.GetVisualDescendants<Control>().Where(x => x is IDisposable).Cast<IDisposable>() is IEnumerable<IDisposable> disp_controls && disp_controls.Any()) {
                //disp_controls.ForEach(x => x.Dispose());
            }
        }

        private static void Window_Opened(object sender, System.EventArgs e) {
            if (sender is not MpAvWindow w) {
                return;
            }

            w.OpenDateTime = DateTime.Now;
            if (w.DataContext is MpICloseWindowViewModel cwvm) {
                cwvm.IsWindowOpen = true;
            }
            Dispatcher.UIThread.Post(async () => {
                // wait for window to activate (if it does)
                await Task.Delay(500);
                OpeningWindows.Remove(w);
            });
            UpdateTopmost();
            //w.Activate();
            //w.Focus();
        }
        private static void Window_Closed(object sender, System.EventArgs e) {
            if (sender is not MpAvWindow w) {
                return;
            }
            OpeningWindows.Remove(w);
            Dispatcher.UIThread.Post(async () => {
                // wait activation change
                await Task.Delay(500);
                _waitingToClose.Remove(w);
            });
            if (w.DataContext is MpIActiveWindowViewModel awvm) {
                awvm.IsWindowActive = false;
            }
            if (w.DataContext is MpICloseWindowViewModel cwvm) {
                cwvm.IsWindowOpen = false;
            }
            UpdateTopmost();
            StartChildLifecycleChangeDelay(w);
        }


        #region Helpers

        private static void AttachAllHandlers(MpAvWindow nw) {
            nw.Opened += Window_Opened;
            nw.Closed += Window_Closed;
            nw.Closing += Window_Closing;
            nw.Activated += Window_Activated;
            nw.Deactivated += Window_Deactivated;
            nw.DataContextChanged += Window_DataContextChanged;
            IDisposable dsp1 = Control.BoundsProperty.Changed.AddClassHandler<MpAvWindow>((x, y) => BoundsChangedHandler(x, y as AvaloniaPropertyChangedEventArgs<Rect>));
            //IDisposable dsp2 = MpAvWindow.TopmostProperty.Changed.AddClassHandler<MpAvWindow>((x, y) => TopmostChangedHandler(x, y as AvaloniaPropertyChangedEventArgs<bool>));
            IDisposable dsp3 = Control.IsVisibleProperty.Changed.AddClassHandler<MpAvWindow>((x, y) => IsVisibleChangedHandler(x, y as AvaloniaPropertyChangedEventArgs<bool>));
            //IDisposable dsp4 = MpAvWindow.WindowStateProperty.Changed.AddClassHandler<MpAvWindow>((x, y) => WindowStateChangedHandler(x, y as AvaloniaPropertyChangedEventArgs<bool>));
            if (_dispLookup.ContainsKey(nw)) {
                MpDebug.Break("Error, window shouldn't already exist here");
            } else {
                _dispLookup.Add(nw, new[] { dsp1, dsp3/*, dsp2, dsp4 */});
            }
            AttachWindowViewModelHandlers(nw);
        }

        private static void DetachAllHandlers(MpAvWindow nw) {
            nw.Opened -= Window_Opened;
            nw.Closed -= Window_Closed;
            nw.Closing -= Window_Closing;
            nw.Activated -= Window_Activated;
            nw.Deactivated -= Window_Deactivated;
            nw.DataContextChanged -= Window_DataContextChanged;
            if (_dispLookup.TryGetValue(nw, out var displ)) {
                displ.ForEach(x => x.Dispose());
                _dispLookup.Remove(nw);
            }
            DetachWindowViewModelHandlers(nw);
        }
        private static void AttachWindowViewModelHandlers(MpAvWindow w) {
            if (w.DataContext is MpAvViewModelBase vmb) {
                vmb.PropertyChanged += WindowViewModel_PropertyChanged;
            }
        }

        private static void DetachWindowViewModelHandlers(MpAvWindow w) {
            if (w.DataContext is MpAvViewModelBase vmb) {
                vmb.PropertyChanged -= WindowViewModel_PropertyChanged;
            }
        }

        private static void WindowViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            if (LocateWindow(sender) is not MpAvWindow cw) {
                return;
            }
            if (sender is MpIWindowStateViewModel wsvm &&
                e.PropertyName == nameof(wsvm.WindowState)) {
                cw.WindowState = wsvm.WindowState;
                return;
            }

            if (sender is MpICloseWindowViewModel cwvm &&
                e.PropertyName == nameof(cwvm.IsWindowOpen)) {
                if (cwvm.IsWindowOpen) {
                    if (!cw.IsActive) {
                        cw.Show();
                    }
                } else {
                    cw.Close();
                }
                return;
            }

            if (sender is MpIWantsTopmostWindowViewModel tmwvm &&
                e.PropertyName == nameof(tmwvm.WantsTopmost)) {
                UpdateTopmost();
                return;
            }

            if (sender is MpIActiveWindowViewModel awvm &&
                e.PropertyName == nameof(awvm.IsWindowActive)) {
                if (!awvm.IsWindowActive) {
                    //cw.WindowState = WindowState.Minimized;
                    return;
                }
                WindowState restore_state = WindowState.Normal;
                if (_restoreStateLookup.TryGetValue(cw, out WindowState last_state)) {
                    restore_state = last_state;
                }
                cw.WindowState = restore_state;
            }
        }


        private static void UpdateTopmost() {
            if (!Dispatcher.UIThread.CheckAccess()) {
                Dispatcher.UIThread.Post(UpdateTopmost);
                return;
            }

#if WINDOWS
            nint last_handle = WinApi.HWND_TOPMOST;
            for (int i = 0; i < TopmostWindowsByZOrder.Count; i++) {
                MpAvWindow w = TopmostWindowsByZOrder[i];
                double scale = w.VisualPixelDensity();
                uint flags = WinApi.SWP_NOACTIVATE | WinApi.SWP_NOMOVE | WinApi.SWP_NOSIZE;
                if (w is MpAvMainWindow && MpAvToolWindow_Win32.IsToolWindow(w.Handle)) {
                    flags = WinApi.SWP_NOACTIVATE | WinApi.SWP_NOMOVE | WinApi.SWP_NOSIZE | WinApi.SWP_FRAMECHANGED | WinApi.SWP_ASYNCWINDOWPOS;
                }
                bool success =
                    WinApi.SetWindowPos(
                        hWnd: w.Handle,
                        hWndInsertAfter: last_handle,
                        X: 0,
                        Y: 0,
                        cx: 0,
                        cy: 0,
                        uFlags: flags);
                if (!success) {
                    //MpConsole.WriteLine($"Failed to set topmost for windown '{w}'");
                }
            }
#else
            //var mw = AllWindows.FirstOrDefault(x => x is MpAvMainWindow);

            //// activate windows wanting top most from highest to lowestpriority

            //if (TopmostWindowsByZOrder.Contains(mw)) {
            //    mw.Topmost = true;
            //} else if (mw != null) {
            //    mw.Topmost = false;
            //}
            if (MainWindow != null) {
                MainWindow.Topmost = TopmostWindowsByZOrder.Contains(MainWindow);
            }

            var wl = TopmostWindowsByZOrder
                .Where(x => x is not MpAvMainWindow);
            foreach(var w in wl) {
                w.Topmost = true;
            }
#endif


        }



        private static async Task ShowWhileAnimatingAsync(MpAvWindow w) {
            if (w.DataContext is not MpIIsAnimatedWindowViewModel adwvm) {
                return;
            }
            adwvm.IsAnimating = true;
            w.Show();

            while (adwvm.IsComplete) {
                await Task.Delay(100);
            }
            adwvm.IsAnimating = false;
            UpdateTopmost();
        }

        private static void StartChildLifecycleChangeDelay(MpAvWindow w) {
            if (w == MainWindow) {
                return;
            }
            Dispatcher.UIThread.Post(async () => {
                await Task.Delay(1000);

                MpAvMainWindowViewModel.Instance.IsMainWindowSilentLocked = false;
            });
        }
        #endregion

        #endregion
    }
}
