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
    public static class MpAvWindowManager {
        #region Private Variables
        private static Dictionary<Window, IEnumerable<IDisposable>> _dispLookup = new Dictionary<Window, IEnumerable<IDisposable>>();
        #endregion

        #region Properties
        public static ObservableCollection<MpAvWindow> AllWindows { get; private set; } = new ObservableCollection<MpAvWindow>();

        public static MpAvWindow MainWindow =>
            AllWindows.FirstOrDefault(x => x is MpAvMainWindow);

        public static MpAvWindow ActiveWindow =>
            AllWindows.FirstOrDefault(x => x.IsActive);


        #endregion

        static MpAvWindowManager() {
            AllWindows.CollectionChanged += AllWindows_CollectionChanged;
        }

        #region Public Methods

        public static Control FindByHashCode(object codeObj) {
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
            return null;
        }

        public static MpAvWindow LocateWindow(object dataContext) {
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

        public static string ToWindowTitleText(this string title) {
            string prefix = string.IsNullOrEmpty(title) ? string.Empty : $"{title} - ";
            return $"{prefix}{MpPrefViewModel.Instance.ApplicationName}";
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
        private static void Nw_DataContextChanged(object sender, EventArgs e) {
            if (sender is Window w) {
                AttachWindowViewModelHandlers(w);
            }
        }


        private static void TopmostWindowViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            if (sender is MpIWantsTopmostWindowViewModel tmwvm) {
                UpdateTopmost();
            }
        }

        private static void ChildWindowViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            if (sender is MpIChildWindowViewModel cwvm) {
                if (e.PropertyName == nameof(cwvm.IsChildWindowOpen)) {
                    var cw = AllWindows.FirstOrDefault(x => x.DataContext == sender);
                    if (cw != null) {
                        if (cwvm.IsChildWindowOpen) {
                            if (!cw.IsActive) {
                                cw.Show();
                            }
                        } else {
                            cw.Close();
                        }
                    }
                }
            }
        }
        private static void Nw_Activated(object sender, EventArgs e) {
            if (sender is Window w) {
                MpMessenger.SendGlobal(MpMessageType.AppWindowActivated);

                if (w.DataContext is MpIActiveWindowViewModel awvm) {
                    awvm.IsActive = true;
                }
                if (w.DataContext is MpIIsAnimatedWindowViewModel adwvm &&
                    adwvm.IsAnimated && !adwvm.IsComplete && !adwvm.IsAnimating) {
                    w.Topmost = true;
                    ShowWhileAnimatingAsync(w).FireAndForgetSafeAsync();
                }
            }
        }
        private static void Nw_Deactivated(object sender, EventArgs e) {
            if (sender is Window w) {
                MpMessenger.SendGlobal(MpMessageType.AppWindowDeactivated);

                if (w.DataContext is MpIActiveWindowViewModel awvm) {
                    awvm.IsActive = false;
                }
                if (w.DataContext is MpIIsAnimatedWindowViewModel adwvm &&
                    adwvm.IsAnimated && !adwvm.IsComplete && !adwvm.IsAnimating) {
                    ShowWhileAnimatingAsync(w).FireAndForgetSafeAsync();
                } else {
                    UpdateTopmost();
                }
            }
        }
        private static void Nw_Opened(object sender, System.EventArgs e) {
            if (sender is Window w) {
                //MpAvMainWindowViewModel.Instance.IsMainWindowSilentLocked = false;

                if (w.DataContext is MpIChildWindowViewModel cwvm) {
                    cwvm.IsChildWindowOpen = true;
                }
                UpdateTopmost();
                //w.Activate();
                //w.Focus();

            }
        }

        private static void Nw_Closing(object sender, WindowClosingEventArgs e) {
            if (sender is Window w) {
                if (w.DataContext is MpIWindowHandlesClosingViewModel whcvm &&
                    whcvm.IsCloseHandled) {
                    // without this check closing evt is called twice in impl 
                    // handler from extra w.Close below
                    return;
                }

                if (w != MainWindow) {
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
        }

        private static void Nw_Closed(object sender, System.EventArgs e) {
            if (sender is Window w) {
                if (w.DataContext is MpIChildWindowViewModel cwvm) {
                    cwvm.IsChildWindowOpen = false;
                }
                StartChildLifecycleChangeDelay(w);
            }
        }

        #region Helpers

        private static void AttachAllHandlers(Window nw) {
            nw.Opened += Nw_Opened;
            nw.Closed += Nw_Closed;
            nw.Closing += Nw_Closing;
            nw.Activated += Nw_Activated;
            nw.Deactivated += Nw_Deactivated;
            nw.DataContextChanged += Nw_DataContextChanged;
            IDisposable dsp1 = Window.BoundsProperty.Changed.AddClassHandler<Window>((x, y) => BoundsChangedHandler(x, y as AvaloniaPropertyChangedEventArgs<Rect>));
            IDisposable dsp2 = Window.TopmostProperty.Changed.AddClassHandler<Window>((x, y) => TopmostChangedHandler(x, y as AvaloniaPropertyChangedEventArgs<bool>));
            IDisposable dsp3 = Window.IsVisibleProperty.Changed.AddClassHandler<Window>((x, y) => IsVisibleChangedHandler(x, y as AvaloniaPropertyChangedEventArgs<bool>));
            if (_dispLookup.ContainsKey(nw)) {
                MpDebug.Break("Error, window shouldn't already exist here");
            } else {
                _dispLookup.Add(nw, new[] { dsp1, dsp2, dsp3 });
            }
            AttachWindowViewModelHandlers(nw);
        }

        private static void DetachAllHandlers(Window nw) {
            nw.Opened -= Nw_Opened;
            nw.Closed -= Nw_Closed;
            nw.Closing -= Nw_Closing;
            nw.Activated -= Nw_Activated;
            nw.Deactivated -= Nw_Deactivated;
            nw.DataContextChanged -= Nw_DataContextChanged;
            if (_dispLookup.TryGetValue(nw, out var displ)) {
                displ.ForEach(x => x.Dispose());
                _dispLookup.Remove(nw);
            }
            DetachWindowViewModelHandlers(nw);
        }
        private static void AttachWindowViewModelHandlers(Window w) {
            if (w.DataContext is MpIWantsTopmostWindowViewModel tmwvm) {
                tmwvm.PropertyChanged += TopmostWindowViewModel_PropertyChanged;
            }
            if (w.DataContext is MpIChildWindowViewModel cwvm) {
                cwvm.PropertyChanged += ChildWindowViewModel_PropertyChanged;
            }
        }

        private static void DetachWindowViewModelHandlers(Window w) {
            if (w.DataContext is MpIWantsTopmostWindowViewModel tmwvm) {
                tmwvm.PropertyChanged -= TopmostWindowViewModel_PropertyChanged;
            }
            if (w.DataContext is MpIChildWindowViewModel cwvm) {
                cwvm.PropertyChanged -= ChildWindowViewModel_PropertyChanged;
            }
        }

        private static void UpdateTopmost() {
            Dispatcher.UIThread.Post(() => {
                if (AllWindows.Any(x => x is MpIIsAnimatedWindowViewModel && (x as MpIIsAnimatedWindowViewModel).IsAnimating)) {
                    // ignore update while animating out
                    return;
                }

                // NOTE only update unowned windows because modal ntf
                var priority_ordered_topmost_wl = AllWindows
                    .Where(x => x.Owner == null && x.WantsTopmost)
                    .OrderByDescending(x => (int)(x.DataContext as MpIWindowViewModel).WindowType);

                priority_ordered_topmost_wl
                    .ForEach((x, idx) => x.Topmost = idx == 0);
            });
        }


        private static async Task ShowWhileAnimatingAsync(Window w) {
            if (w.DataContext is MpIIsAnimatedWindowViewModel adwvm) {
                adwvm.IsAnimating = true;
                w.Show();

                while (adwvm.IsComplete) {
                    await Task.Delay(100);
                }
                adwvm.IsAnimating = false;
                UpdateTopmost();
            }
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


        #region Helper Extensions

        private static bool IsSelfOrDescendantWindowWantTopmost(this MpAvWindow w) {
            if (w == null) {
                return false;
            }
            if (w.WantsTopmost) {
                return true;
            }
            return
                AllWindows
                .Where(x => x.Owner == w)
                .Any(x => x.IsSelfOrDescendantWindowWantTopmost());
        }

        public static void SetTopmostForSelfAndAllDescendants(this MpAvWindow w, bool is_topmost) {
            if (w == null) {
                return;
            }
            if (is_topmost) {
                if (w.WantsTopmost) {
                    w.Topmost = true;
                }
            } else {
                w.GetDescendantWindows(true).ForEach(x => x.Topmost = false);
            }

        }

        private static int GetWindowOwnerDepth(this Window w) {
            int depth = 0;
            if (w == null) {
                return depth;
            }
            while (true) {
                w = w.Owner as Window;
                if (w == null) {
                    break;
                }
                depth++;
            }
            return depth;
        }

        private static IEnumerable<MpAvWindow> GetDescendantWindows(this MpAvWindow w, bool include_self = true) {
            List<MpAvWindow> wl = new List<MpAvWindow>();
            if (w == null) {
                return wl;
            }
            if (include_self) {
                wl.Add(w);
            }

            foreach (var cw in AllWindows.Where(x => x.Owner == w)) {
                wl.AddRange(cw.GetDescendantWindows(true));
            }
            return wl.Distinct();
        }
        #endregion

        #endregion

        #endregion
    }
}
