using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Avalonia;
using System;
using System.Reactive.Linq;

namespace MonkeyPaste.Avalonia {
    public static class MpAvBoundsObserverExtension {

        #region Constructors
        static MpAvBoundsObserverExtension() {
            IsEnabledProperty.Changed.AddClassHandler<Control>((x, y) => HandleIsEnabledChanged(x, y));
        }

        #endregion

        #region Properties

        #region ObservedBounds AvaloniaProperty
        public static MpRect GetObservedBounds(AvaloniaObject obj) {
            return obj.GetValue(ObservedBoundsProperty);
        }

        public static void SetObservedBounds(AvaloniaObject obj, MpRect value) {
            obj.SetValue(ObservedBoundsProperty, value);
        }

        public static readonly AttachedProperty<MpRect> ObservedBoundsProperty =
            AvaloniaProperty.RegisterAttached<object, Control, MpRect>(
                "ObservedBounds",
               null,
                false,
                BindingMode.TwoWay);

        #endregion

        #region ToScreen AvaloniaProperty
        public static bool GetToScreen(AvaloniaObject obj) {
            return obj.GetValue(ToScreenProperty);
        }

        public static void SetToScreen(AvaloniaObject obj, bool value) {
            obj.SetValue(ToScreenProperty, value);
        }

        public static readonly AttachedProperty<bool> ToScreenProperty =
            AvaloniaProperty.RegisterAttached<object, Control, bool>(
                "ToScreen",
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

        private static void HandleIsEnabledChanged(Control element, AvaloniaPropertyChangedEventArgs e) {
            if (e.NewValue is bool isEnabledVal && isEnabledVal) {
                if (element is Control control) {
                    control.DetachedFromVisualTree += DetachedFromVisualHandler;
                    control.AttachedToVisualTree += Control_AttachedToVisualTree;
                    if (control.IsInitialized) {
                        Control_AttachedToVisualTree(control, null);
                    }


                }
            } else {
                DetachedFromVisualHandler(element, null);
            }

        }

        private static void Control_AttachedToVisualTree(object sender, VisualTreeAttachmentEventArgs e) {
            var control = sender as Control;
            if (control == null) {
                return;
            }

            if (control.GetVisualAncestor<ListBoxItem>() is ListBoxItem lbi) {
                // workaround since ListBoxItem is abstract in clip tray..
                var boundsObserver = lbi.GetObservable(ListBoxItem.BoundsProperty);
                boundsObserver.Subscribe(x => BoundsChangedHandler(control));
            } else {
                var boundsObserver = control.GetObservable(Control.BoundsProperty);
                boundsObserver.Subscribe(x => BoundsChangedHandler(control));
            }
        }
        private static void DetachedFromVisualHandler(object s, VisualTreeAttachmentEventArgs e) {
            if (s is Control control) {
                control.DetachedFromVisualTree -= DetachedFromVisualHandler;
                control.AttachedToVisualTree -= Control_AttachedToVisualTree;
            }
        }

        private static void BoundsChangedHandler(Control control) {
            MpRect new_bounds = control.Bounds.ToPortableRect();
            if (control.GetVisualAncestor<ListBoxItem>() is ListBoxItem lbi) {
                //SetObservedBounds_safe(control, lbi);
                new_bounds = lbi.Bounds.ToPortableRect();
            }
            if (GetToScreen(control)) {
                // this has a relative to control
                new_bounds.TranslateOrigin(null);
            }

            SetObservedBounds(control, new_bounds);
        }
        #endregion

        #endregion
    }
}
