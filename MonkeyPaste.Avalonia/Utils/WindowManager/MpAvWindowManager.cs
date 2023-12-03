using Avalonia;
using Avalonia.Controls;
using Avalonia.Threading;
using Avalonia.VisualTree;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Avalonia;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace MonkeyPaste.Avalonia {

    public interface MpIWindowStateViewModel : MpIWindowViewModel {
        WindowState WindowState { get; set; }
    }

    public static class MpAvWindowManager {
        #region Private Variables
        private static Dictionary<Window, IEnumerable<IDisposable>> _dispLookup = new Dictionary<Window, IEnumerable<IDisposable>>();
        private static Dictionary<Window, WindowState> _restoreStateLookup = new Dictionary<Window, WindowState>();
        private static List<MpAvWindow> _waitingToClose = new List<MpAvWindow>();
        #endregion

        #region Properties

        public static Screens Screens =>
            AllWindows.Any() ? AllWindows.FirstOrDefault().Screens : null;
        public static ObservableCollection<MpAvWindow> AllWindows { get; private set; } = new ObservableCollection<MpAvWindow>();

        public static MpAvWindow MainWindow =>
            AllWindows.FirstOrDefault(x => x is MpAvMainWindow);

        public static MpAvWindow ActiveWindow =>
            AllWindows.FirstOrDefault(x => x.IsActive);
        public static MpAvWindow LastActiveWindow =>
            AllWindows.OrderByDescending(x => x.LastActiveDateTime).FirstOrDefault();

        public static IReadOnlyList<MpAvWindow> Notifications =>
            AllWindows.Where(x => x.DataContext is MpAvNotificationViewModelBase).ToList();

        public static IReadOnlyList<MpAvWindow> ToastNotifications =>
            Notifications.Where(x => x.Classes.Contains("toast")).ToList();

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
                    return nint.Zero;
                }
                return w.TryGetPlatformHandle().Handle;
            }
        }

        #endregion

        #region Events
        public static event EventHandler NotificationWindowsChanged;
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

        public static MpAvWindow LocateWindow(MpPoint gmp) {
            return AllWindows.FirstOrDefault(x => x.ScaledScreenRect().Contains(gmp));
        }
        public static MpAvWindow LocateWindow(object dataContext) {
            if (!Dispatcher.UIThread.CheckAccess()) {
                var result = Dispatcher.UIThread.Invoke(() => LocateWindow(dataContext));
                return result;
            }
            if (AllWindows.FirstOrDefault(x => x.DataContext == dataContext) is MpAvWindow w) {
                return w;
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

        #endregion

        #region Private Methods

        private static void AllWindows_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e) {
            if (e.NewItems != null) {
                foreach (Window nw in e.NewItems) {
                    AttachAllHandlers(nw);
                }
            }
            if (e.OldItems != null) {
                foreach (Window ow in e.OldItems) {
                    DetachAllHandlers(ow);
                }
            }
        }
        private static void BoundsChangedHandler(Window w, AvaloniaPropertyChangedEventArgs<Rect> e) {
            if (w.DataContext is MpIWindowBoundsObserverViewModel wbovm) {
                var oldAndNewVals = e.GetOldAndNewValue<Rect>();
                wbovm.LastBounds = oldAndNewVals.oldValue.ToPortableRect();
                wbovm.Bounds = oldAndNewVals.newValue.ToPortableRect();
            }
        }

        private static void TopmostChangedHandler(Window w, AvaloniaPropertyChangedEventArgs<bool> e) {
            UpdateTopmost();
        }


        private static void IsVisibleChangedHandler(Window w, AvaloniaPropertyChangedEventArgs<bool> e) {
            UpdateTopmost();
        }

        private static void WindowStateChangedHandler(Window w, AvaloniaPropertyChangedEventArgs<bool> e) {
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
                w.Topmost = true;
                ShowWhileAnimatingAsync(w).FireAndForgetSafeAsync();
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
                StartChildLifecycleChangeDelay(w);
            }
        }
        private static void Window_Opened(object sender, System.EventArgs e) {
            if (sender is not MpAvWindow w) {
                return;
            }
            // close any waiting windows (startup bug)
            _waitingToClose.ForEach(x => x.Close());
            w.OpenDateTime = DateTime.Now;
            //MpAvMainWindowViewModel.Instance.IsMainWindowSilentLocked = false;

            if (w.DataContext is MpICloseWindowViewModel cwvm) {
                cwvm.IsWindowOpen = true;
            }
            UpdateTopmost();
            //w.Activate();
            //w.Focus();
        }

        private static void Nw_Closing(object sender, WindowClosingEventArgs e) {
            if (sender is not MpAvWindow w) {
                return;
            }
            if (w.DataContext is MpIWindowHandlesClosingViewModel whcvm &&
                    whcvm.IsWindowCloseHandled) {
                // without this check closing evt is called twice in impl 
                // handler from extra w.Close below
                return;
            }

            if (w != MainWindow) {
                if (AllWindows.Count <= 1 && !_waitingToClose.Contains(w)) {
                    // occurs in startup if loader was previously set to always hide and password attempt fails
                    // when pwd window closes theres no main window so app closes
                    // need to hide window and store ref and wait for new window before closing
                    e.Cancel = true;
                    w.Hide();
                    _waitingToClose.Add(w);
                    return;
                }
                if (!MpAvMainWindowViewModel.Instance.IsMainWindowSilentLocked) {
                    MpAvMainWindowViewModel.Instance.IsMainWindowSilentLocked = true;
                    e.Cancel = true;
                    object dresult = null;
                    if (w is MpAvWindow avw) {
                        dresult = avw.DialogResult;
                    }
                    w.Close(dresult);
                    return;
                }
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
            if (w.GetVisualDescendants<Control>().Where(x => x is IDisposable).Cast<IDisposable>() is IEnumerable<IDisposable> disp_controls) {
                disp_controls.ForEach(x => x.Dispose());
            }
        }

        private static void Nw_Closed(object sender, System.EventArgs e) {
            if (sender is not MpAvWindow w) {
                return;
            }
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

        private static void AttachAllHandlers(Window nw) {
            nw.Opened += Window_Opened;
            nw.Closed += Nw_Closed;
            nw.Closing += Nw_Closing;
            nw.Activated += Window_Activated;
            nw.Deactivated += Window_Deactivated;
            nw.DataContextChanged += Window_DataContextChanged;
            IDisposable dsp1 = Window.BoundsProperty.Changed.AddClassHandler<Window>((x, y) => BoundsChangedHandler(x, y as AvaloniaPropertyChangedEventArgs<Rect>));
            IDisposable dsp2 = Window.TopmostProperty.Changed.AddClassHandler<Window>((x, y) => TopmostChangedHandler(x, y as AvaloniaPropertyChangedEventArgs<bool>));
            IDisposable dsp3 = Window.IsVisibleProperty.Changed.AddClassHandler<Window>((x, y) => IsVisibleChangedHandler(x, y as AvaloniaPropertyChangedEventArgs<bool>));
            IDisposable dsp4 = Window.WindowStateProperty.Changed.AddClassHandler<Window>((x, y) => WindowStateChangedHandler(x, y as AvaloniaPropertyChangedEventArgs<bool>));
            if (_dispLookup.ContainsKey(nw)) {
                MpDebug.Break("Error, window shouldn't already exist here");
            } else {
                _dispLookup.Add(nw, new[] { dsp1, dsp2, dsp3, dsp4 });
            }
            AttachWindowViewModelHandlers(nw);

            if (nw != null && nw.DataContext is MpAvNotificationViewModelBase) {
                NotificationWindowsChanged?.Invoke(nameof(MpAvWindowManager), EventArgs.Empty);
            }
        }

        private static void DetachAllHandlers(Window nw) {
            nw.Opened -= Window_Opened;
            nw.Closed -= Nw_Closed;
            nw.Closing -= Nw_Closing;
            nw.Activated -= Window_Activated;
            nw.Deactivated -= Window_Deactivated;
            nw.DataContextChanged -= Window_DataContextChanged;
            if (_dispLookup.TryGetValue(nw, out var displ)) {
                displ.ForEach(x => x.Dispose());
                _dispLookup.Remove(nw);
            }
            DetachWindowViewModelHandlers(nw);

            if (nw != null && nw.DataContext is MpAvNotificationViewModelBase) {
                NotificationWindowsChanged?.Invoke(nameof(MpAvWindowManager), EventArgs.Empty);
            }
        }
        private static void AttachWindowViewModelHandlers(Window w) {
            if (w.DataContext is MpAvViewModelBase vmb) {
                vmb.PropertyChanged += WindowViewModel_PropertyChanged;
            }
        }

        private static void DetachWindowViewModelHandlers(Window w) {
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
            if (AllWindows.Any(x => x is MpIIsAnimatedWindowViewModel && (x as MpIIsAnimatedWindowViewModel).IsAnimating)) {
                // ignore update while animating out
                return;
            }

            // NOTE only update unowned windows because modal ntf

            // get non-minimzed windows wanting topmost ordered by least priority
            var priority_ordered_topmost_wl = AllWindows
                .Where(x => x.Owner == null && x.WantsTopmost && x.WindowState != WindowState.Minimized)
                .OrderBy(x => x.IsActive)
                .ThenBy(x => (int)x.BindingContext.WindowType);

            // activate windows wanting top most for lowest to highest priority
            priority_ordered_topmost_wl
                //.ForEach((x, idx) => x.Topmost = idx == 0);
                .ForEach(x => x.Topmost = true);
        }


        private static async Task ShowWhileAnimatingAsync(Window w) {
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

        private static void StartChildLifecycleChangeDelay(Window w) {
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
