using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using System.Diagnostics;

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

        private static void HandleIsEnabledChanged(IAvaloniaObject element, AvaloniaPropertyChangedEventArgs e) {
            if (e.NewValue is bool isEnabledVal && isEnabledVal) {
                if (element is Control control) {
                    if (control.IsInitialized) {
                        AttachedToVisualHandler(control, null);
                    } else {
                        control.AttachedToVisualTree += AttachedToVisualHandler;

                    }
                }
            } else {
                DetachedToVisualHandler(element, null);
            }

            void AttachedToVisualHandler(object? s, VisualTreeAttachmentEventArgs? e) {
                if (s is Control control) {
                    if (e == null) {
                        control.AttachedToVisualTree += AttachedToVisualHandler;
                    }
                    control.DetachedFromVisualTree += DetachedToVisualHandler;
                    control.EffectiveViewportChanged += Control_EffectiveViewportChanged;
                    Control_EffectiveViewportChanged(control, null);
                }
            }
            void DetachedToVisualHandler(object? s, VisualTreeAttachmentEventArgs? e) {
                if (s is Control control) {
                    control.AttachedToVisualTree -= AttachedToVisualHandler;
                    control.DetachedFromVisualTree -= DetachedToVisualHandler;
                    control.EffectiveViewportChanged -= Control_EffectiveViewportChanged;
                }
            }

            void Control_EffectiveViewportChanged(object sender, global::Avalonia.Layout.EffectiveViewportChangedEventArgs? e) {
                if (sender is Control control) {
                    SetObservedWidth(control, control.Bounds.Width);
                    SetObservedHeight(control, control.Bounds.Height);
                    //if(control.Name == "ClipTrayContainerBorder") {
                    //    MpAvClipTrayViewModel.Instance.OnPropertyChanged(nameof(MpAvClipTrayViewModel.Instance.ClipTrayScreenWidth));
                    //    MpAvClipTrayViewModel.Instance.OnPropertyChanged(nameof(MpAvClipTrayViewModel.Instance.ClipTrayScreenHeight));
                    //}
                }
            }
        }



        #endregion

        #endregion
    }
}
