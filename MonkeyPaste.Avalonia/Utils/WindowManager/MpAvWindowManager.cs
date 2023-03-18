using Avalonia;
using Avalonia.Controls;
using Avalonia.Threading;
using Avalonia.VisualTree;
using CefNet;
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
                if (e.PropertyName == nameof(cwvm.IsOpen)) {
                    var cw = AllWindows.FirstOrDefault(x => x.DataContext == sender);
                    if (cw != null) {
                        if (cwvm.IsOpen) {
                            // open isn't needed anywhere
                        } else {
                            cw.Close();
                        }
                    }
                }
            }
        }
        private static void Nw_Activated(object sender, EventArgs e) {
            if (sender is Window w) {
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
                MpAvMainWindowViewModel.Instance.IsMainWindowSilentLocked = false;

                if (w.DataContext is MpIChildWindowViewModel cwvm) {
                    cwvm.IsOpen = true;
                }
                if (w.DataContext is MpIWantsTopmostWindowViewModel tmwvm &&
                    tmwvm.WantsTopmost) {
                    w.Topmost = true;
                }
                if (w.DataContext is MpIIsAnimatedWindowViewModel adwvm &&
                    adwvm.IsAnimated) {
                    w.Topmost = true;
                }
            }
        }

        private static void Nw_Closing(object sender, WindowClosingEventArgs e) {
            if (sender is Window w) {

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
                    cwvm.IsOpen = false;
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
            if (AllWindows.Any(x => x is MpIIsAnimatedWindowViewModel && (x as MpIIsAnimatedWindowViewModel).IsAnimating)) {
                // ignore update while animating out
                return;
            }

            var topmost_w = GetMostSignificantTopmostWindow();
            if (topmost_w == null) {
                return;
            }
            topmost_w.Topmost = true;
        }

        private static Window GetMostSignificantTopmostWindow() {
            return AllWindows
                .Where(x => (x.DataContext is MpIWantsTopmostWindowViewModel) && (x.DataContext as MpIWantsTopmostWindowViewModel).WantsTopmost)
                .OrderByDescending(x => (int)(x.DataContext as MpIWindowViewModel).WindowType)
                .FirstOrDefault();
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

        #endregion

        #endregion
    }
}
