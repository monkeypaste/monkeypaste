using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Threading;
using Avalonia.VisualTree;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Avalonia;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace MonkeyPaste.Avalonia {
    public static class MpAvScrollBarVisibilityExtension {
        #region Private Variables

        private static Dictionary<ScrollViewer, Control> _enabledControlLookup = new Dictionary<ScrollViewer, Control>();
        #endregion
        static MpAvScrollBarVisibilityExtension() {
            IsEnabledProperty.Changed.AddClassHandler<Control>((x, y) => HandleIsEnabledChanged(x, y));
        }
        #region Properties

        #region IsHorizontalScrollbarVisible AvaloniaProperty
        public static bool GetIsHorizontalScrollbarVisible(AvaloniaObject obj) {
            return obj.GetValue(IsHorizontalScrollbarVisibleProperty);
        }

        public static void SetIsHorizontalScrollbarVisible(AvaloniaObject obj, bool value) {
            obj.SetValue(IsHorizontalScrollbarVisibleProperty, value);
        }

        public static readonly AttachedProperty<bool> IsHorizontalScrollbarVisibleProperty =
            AvaloniaProperty.RegisterAttached<object, Control, bool>(
                "IsHorizontalScrollbarVisible",
                false,
                false);

        #endregion

        #region IsVerticalScrollbarVisible AvaloniaProperty
        public static bool GetIsVerticalScrollbarVisible(AvaloniaObject obj) {
            return obj.GetValue(IsVerticalScrollbarVisibleProperty);
        }

        public static void SetIsVerticalScrollbarVisible(AvaloniaObject obj, bool value) {
            obj.SetValue(IsVerticalScrollbarVisibleProperty, value);
        }

        public static readonly AttachedProperty<bool> IsVerticalScrollbarVisibleProperty =
            AvaloniaProperty.RegisterAttached<object, Control, bool>(
                "IsVerticalScrollbarVisible",
                false,
                false);

        #endregion

        #region IsEnabled AvaloniaProperty
        public static bool GetIsEnabled(AvaloniaObject obj) {
            return obj.GetValue(IsEnabledProperty);
        }

        public static void SetIsEnabled(AvaloniaObject obj, bool value) {
            obj.SetValue(IsEnabledProperty, value);
        }

        public static readonly AttachedProperty<bool> IsEnabledProperty =
            AvaloniaProperty.RegisterAttached<object, Control, bool>(
                "IsEnabled",
                false,
                false);

        private static void HandleIsEnabledChanged(Control control, AvaloniaPropertyChangedEventArgs e) {
            if (e.NewValue is bool isEnabledVal) {
                if (isEnabledVal) {
                    control.AttachedToVisualTree += Control_AttachedToVisualTree;
                    control.DetachedFromVisualTree += Control_DetachedFromVisualTree;
                    if (control.IsAttachedToVisualTree()) {
                        Control_AttachedToVisualTree(control, null);
                    }
                } else {
                    Control_DetachedFromVisualTree(control, null);
                }
            }
        }

        private static void Control_DetachedFromVisualTree(object sender, VisualTreeAttachmentEventArgs e) {
            var control = sender as Control;
            if (control == null) {
                return;
            }
            if (_enabledControlLookup.ContainsValue(control)) {
                var kvp = _enabledControlLookup.FirstOrDefault(x => x.Value == control);
                _enabledControlLookup.Remove(kvp.Key);
            }
            control.AttachedToVisualTree += Control_AttachedToVisualTree;
            control.DetachedFromVisualTree += Control_DetachedFromVisualTree;
        }

        private static void Control_AttachedToVisualTree(object sender, VisualTreeAttachmentEventArgs e) {
            var control = sender as Control;
            if (control == null) {
                return;
            }
            Dispatcher.UIThread.Post(async () => {
                var sw = Stopwatch.StartNew();
                ScrollViewer control_sv = control.GetVisualDescendant<ScrollViewer>();
                while (control_sv == null) {
                    await Task.Delay(100);
                    control_sv = control.GetVisualDescendant<ScrollViewer>();
                    if (sw.ElapsedMilliseconds > 10_000) {
                        MpDebug.Break("Locate scrollviewer timeout");
                        return;
                    }
                }
                _enabledControlLookup.AddOrReplace(control_sv, control);

                sw = Stopwatch.StartNew();
                var sbl = control.GetVisualDescendants<ScrollBar>();
                while (true) {
                    if (sbl.Count() == 2) {
                        break;
                    }
                    sbl = control.GetVisualDescendants<ScrollBar>();
                    if (sw.ElapsedMilliseconds > 10_000) {
                        MpDebug.Break("Locate scrollbar timeout");
                        return;
                    }
                }
                sbl.ForEach(x =>
                    x.GetObservable(ScrollBar.IsExpandedProperty)
                    .Subscribe(value => OnIsScrollBarExpandedChanged(x)));
            });
        }


        #endregion

        #endregion

        private static void OnIsScrollBarExpandedChanged(ScrollBar sb) {
            var sv = sb.GetVisualAncestor<ScrollViewer>();
            if (_enabledControlLookup.TryGetValue(sv, out Control c)) {
                if (sb.Orientation == Orientation.Horizontal) {
                    SetIsHorizontalScrollbarVisible(c, sb.IsExpanded);
                } else {
                    SetIsVerticalScrollbarVisible(c, sb.IsExpanded);
                }

            }

        }

    }
}
