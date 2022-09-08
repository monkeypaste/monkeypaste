using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.VisualTree;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Avalonia;
using System.Diagnostics;

namespace MonkeyPaste.Avalonia {
    public static class MpAvBoundsObserverExtension {
        static MpAvBoundsObserverExtension() {
            IsEnabledProperty.Changed.AddClassHandler<Control>((x, y) => HandleIsEnabledChanged(x, y));
        }
        #region Properties

        #region RelativeTo AvaloniaProperty
        public static IVisual GetRelativeTo(AvaloniaObject obj) {
            return obj.GetValue(RelativeToProperty);
        }

        public static void SetRelativeTo(AvaloniaObject obj, IVisual value) {
            obj.SetValue(RelativeToProperty, value);
        }

        public static readonly AttachedProperty<IVisual> RelativeToProperty =
            AvaloniaProperty.RegisterAttached<object, Control, IVisual>(
                "RelativeTo",
               null,
                false);

        #endregion

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
                    MpRect bounds = control.Bounds.ToPortableRect();
                    if (GetRelativeTo(control) is IVisual relativeTo) {
                        var relativeTo_origin = control.TranslatePoint(new Point(0,0), relativeTo).Value;
                        bounds = new MpRect(relativeTo_origin.ToPortablePoint(), bounds.Size);
                    }
                    SetObservedBounds(control, control.Bounds.ToPortableRect());
                }
            }
        }



        #endregion

        #endregion
    }
}
