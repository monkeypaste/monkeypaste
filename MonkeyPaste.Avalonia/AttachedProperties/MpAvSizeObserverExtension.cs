using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using System;

namespace MonkeyPaste.Avalonia {
    public static class MpAvSizeObserverExtension {
        static MpAvSizeObserverExtension() {
            IsEnabledProperty.Changed.AddClassHandler<Control>((x, y) => HandleIsEnabledChanged(x, y));
        }
        #region Properties

        #region ObservedWidth AvaloniaProperty
        public static double GetObservedWidth(AvaloniaObject obj) {
            return obj.GetValue(ObservedWidthProperty);
        }

        public static void SetObservedWidth(AvaloniaObject obj, double value) {
            obj.SetValue(ObservedWidthProperty, value);
        }

        public static readonly AttachedProperty<double> ObservedWidthProperty =
            AvaloniaProperty.RegisterAttached<object, Control, double>(
                "ObservedWidth",
                0.0d,
                false,
                BindingMode.OneWayToSource);

        #endregion

        #region ObservedHeight AvaloniaProperty
        public static double GetObservedHeight(AvaloniaObject obj) {
            return obj.GetValue(ObservedHeightProperty);
        }

        public static void SetObservedHeight(AvaloniaObject obj, double value) {
            obj.SetValue(ObservedHeightProperty, value);
        }

        public static readonly AttachedProperty<double> ObservedHeightProperty =
            AvaloniaProperty.RegisterAttached<object, Control, double>(
                "ObservedHeight",
                0.0d,
                false,
                BindingMode.OneWayToSource);

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
                    control.AttachedToVisualTree += AttachedToVisualHandler;
                    control.DetachedFromVisualTree += DetachedFromVisualHandler;
                    if (control.IsInitialized) {
                        AttachedToVisualHandler(control, null);
                    }
                }
            } else {
                DetachedFromVisualHandler(element, null);
            }


        }

        private static void AttachedToVisualHandler(object s, VisualTreeAttachmentEventArgs? e) {
            if (s is Control control) {
                if (e == null) {
                    control.AttachedToVisualTree += AttachedToVisualHandler;
                }
                control.DetachedFromVisualTree += DetachedFromVisualHandler;
                control.EffectiveViewportChanged += Control_EffectiveViewportChanged;
                control.GetObservable(Control.BoundsProperty).Subscribe(value => Control_EffectiveViewportChanged(control, null));
                Control_EffectiveViewportChanged(control, null);
            }
        }
        private static void DetachedFromVisualHandler(object s, VisualTreeAttachmentEventArgs? e) {
            if (s is Control control) {
                control.AttachedToVisualTree -= AttachedToVisualHandler;
                control.DetachedFromVisualTree -= DetachedFromVisualHandler;
                control.EffectiveViewportChanged -= Control_EffectiveViewportChanged;
            }
        }

        private static void Control_EffectiveViewportChanged(object sender, global::Avalonia.Layout.EffectiveViewportChangedEventArgs? e) {
            if (sender is Control control) {
                if ((int)control.Bounds.Width != (int)GetObservedWidth(control)) {
                    SetObservedWidth(control, control.Bounds.Width);
                }
                if ((int)control.Bounds.Height != (int)GetObservedHeight(control)) {
                    SetObservedHeight(control, control.Bounds.Height);
                }

                //if(control.Name == "ClipTrayContainerBorder") {
                //    MpAvClipTrayViewModel.Instance.OnPropertyChanged(nameof(MpAvClipTrayViewModel.Instance.QueryTrayScreenWidth));
                //    MpAvClipTrayViewModel.Instance.OnPropertyChanged(nameof(MpAvClipTrayViewModel.Instance.QueryTrayScreenHeight));
                //}
            }
        }

        #endregion

        #endregion
    }
}
