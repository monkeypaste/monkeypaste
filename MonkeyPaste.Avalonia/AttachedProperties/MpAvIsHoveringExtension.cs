using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Threading;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Avalonia;
using MonkeyPaste.Common.Plugin;
using System;
using System.Collections.Generic;

namespace MonkeyPaste.Avalonia {
    public static class MpAvIsHoveringExtension {
        #region Private Variables
        #endregion

        #region Statics
        static MpAvIsHoveringExtension() {
            IsEnabledProperty.Changed.AddClassHandler<Control>((x, y) => HandleIsEnabledChanged(x, y));
        }

        #endregion

        #region Properties

        #region IsHovering AvaloniaProperty
        public static bool GetIsHovering(AvaloniaObject obj) {
            return obj.GetValue(IsHoveringProperty);
        }

        public static void SetIsHovering(AvaloniaObject obj, bool value) {
            obj.SetValue(IsHoveringProperty, value);
        }

        public static readonly AttachedProperty<bool> IsHoveringProperty =
            AvaloniaProperty.RegisterAttached<object, Control, bool>(
                "IsHovering",
                false,
                false,
                BindingMode.TwoWay);

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
                    control.Loaded += Control_Loaded;
                    if (control.IsLoaded) {
                        Control_Loaded(control, null);
                    }
                }
            } else {
                Control_Unloaded(element, null);
            }


        }


        #endregion

        #endregion

        #region Private Methods

        private static void Control_Loaded(object sender, global::Avalonia.Interactivity.RoutedEventArgs e) {
            if (sender is Control control) {
                control.Unloaded += Control_Unloaded;
                control.PointerEntered += PointerEnterHandler;
                control.PointerExited += PointerLeaveHandler;
                control.AddHandler(Control.PointerPressedEvent, Control_PointerPressed, RoutingStrategies.Tunnel);
                control.AddHandler(Control.PointerReleasedEvent, Control_PointerReleased, RoutingStrategies.Tunnel); 
            }
        }


        private static void Control_Unloaded(object sender, global::Avalonia.Interactivity.RoutedEventArgs e) {
            if (sender is Control control) {
                control.Loaded -= Control_Loaded;
                control.Unloaded -= Control_Unloaded;
                control.PointerEntered -= PointerEnterHandler;
                control.PointerExited -= PointerLeaveHandler;
                control.PointerPressed -= Control_PointerPressed;
                control.PointerReleased -= Control_PointerReleased;
            }
        }

        private static void PointerEnterHandler(object sender, PointerEventArgs e) {
            if (sender is not Control control) {
                return;
            }
            SetIsHovering(control, true);
        }


        private static void PointerLeaveHandler(object sender, PointerEventArgs e) {
            if (sender is not Control control) {
                return;
            }
            SetIsHovering(control, false);
        }

        private static void Control_PointerReleased(object sender, PointerReleasedEventArgs e) {
            if (sender is not Control control) {
                return;
            }
            SetIsHovering(control, false);
        }

        private static void Control_PointerPressed(object sender, PointerPressedEventArgs e) {
            if (sender is not Control control) {
                return;
            }
            SetIsHovering(control, true);
        }
        #endregion
    }

}
