using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Interactivity;
using System.Windows.Input;
using System.Linq;
using System;
using Avalonia.Media;
using Avalonia.VisualTree;
using MonkeyPaste.Common.Avalonia;
using System.Runtime.Intrinsics.Arm;
using Avalonia.Media.Immutable;
using Avalonia.Controls.Shapes;
using System.Diagnostics;

namespace MonkeyPaste.Avalonia {
    public static class MpAvCustomWrapPanelExtension {
        static MpAvCustomWrapPanelExtension() {
            IsEnabledProperty.Changed.AddClassHandler<Control>((x, y) => HandleIsEnabledChanged(x, y));
        }

        #region LeftPressCommand AvaloniaProperty
        public static ICommand GetLeftPressCommand(AvaloniaObject obj) {
            return obj.GetValue(LeftPressCommandProperty);
        }

        public static void SetLeftPressCommand(AvaloniaObject obj, ICommand value) {
            obj.SetValue(LeftPressCommandProperty, value);
        }

        public static readonly AttachedProperty<ICommand> LeftPressCommandProperty =
            AvaloniaProperty.RegisterAttached<object, Control, ICommand>(
                "LeftPressCommand",
                null,
                false);

        #endregion

        #region RightPressCommand AvaloniaProperty
        public static ICommand GetRightPressCommand(AvaloniaObject obj) {
            return obj.GetValue(RightPressCommandProperty);
        }

        public static void SetRightPressCommand(AvaloniaObject obj, ICommand value) {
            obj.SetValue(RightPressCommandProperty, value);
        }

        public static readonly AttachedProperty<ICommand> RightPressCommandProperty =
            AvaloniaProperty.RegisterAttached<object, Control, ICommand>(
                "RightPressCommand",
                null,
                false);

        #endregion

        #region DoubleLeftPressCommand AvaloniaProperty
        public static ICommand GetDoubleLeftPressCommand(AvaloniaObject obj) {
            return obj.GetValue(DoubleLeftPressCommandProperty);
        }

        public static void SetDoubleLeftPressCommand(AvaloniaObject obj, ICommand value) {
            obj.SetValue(DoubleLeftPressCommandProperty, value);
        }

        public static readonly AttachedProperty<ICommand> DoubleLeftPressCommandProperty =
            AvaloniaProperty.RegisterAttached<object, Control, ICommand>(
                "DoubleLeftPressCommand",
                null,
                false);

        #endregion

        #region LeftPressCommandParameter AvaloniaProperty
        public static object GetLeftPressCommandParameter(AvaloniaObject obj) {
            return obj.GetValue(LeftPressCommandParameterProperty);
        }

        public static void SetLeftPressCommandParameter(AvaloniaObject obj, object value) {
            obj.SetValue(LeftPressCommandParameterProperty, value);
        }

        public static readonly AttachedProperty<object> LeftPressCommandParameterProperty =
            AvaloniaProperty.RegisterAttached<object, Control, object>(
                "LeftPressCommandParameter",
                null,
                false);

        #endregion

        #region RightPressCommandParameter AvaloniaProperty
        public static object GetRightPressCommandParameter(AvaloniaObject obj) {
            return obj.GetValue(RightPressCommandParameterProperty);
        }

        public static void SetRightPressCommandParameter(AvaloniaObject obj, object value) {
            obj.SetValue(RightPressCommandParameterProperty, value);
        }

        public static readonly AttachedProperty<object> RightPressCommandParameterProperty =
            AvaloniaProperty.RegisterAttached<object, Control, object>(
                "RightPressCommandParameter",
                null,
                false);

        #endregion

        #region PanelName AvaloniaProperty
        public static string GetPanelName(AvaloniaObject obj) {
            return obj.GetValue(PanelNameProperty);
        }

        public static void SetPanelName(AvaloniaObject obj, string value) {
            obj.SetValue(PanelNameProperty, value);
        }

        public static readonly AttachedProperty<string> PanelNameProperty =
            AvaloniaProperty.RegisterAttached<object, Control, string>(
                "PanelName",
                null,
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

        private static void HandleIsEnabledChanged(IAvaloniaObject element, AvaloniaPropertyChangedEventArgs e) {
            if(e.NewValue is bool isEnabledVal && isEnabledVal) {
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

            

        }
        private static void AttachedToVisualHandler(object? s, VisualTreeAttachmentEventArgs? e) {
            if (s is Control control) {
                control.DetachedFromVisualTree += DetachedToVisualHandler;
                control.PointerPressed += Control_PointerPressed;

                if (e == null) {
                    control.AttachedToVisualTree += AttachedToVisualHandler;
                }
                //if(control is ICommandSource cs && GetLeftPressCommand(control) is ICommand l_cmd) {
                //    cs.Command = GetLeftPressCommand(control);
                //    cs.
                //}
            }
        }

        private static void DetachedToVisualHandler(object? s, VisualTreeAttachmentEventArgs? e) {
            if (s is Control control) {
                control.AttachedToVisualTree -= AttachedToVisualHandler;
                control.DetachedFromVisualTree -= DetachedToVisualHandler;
                control.PointerPressed -= Control_PointerPressed;
            }
        }

        private static void Control_PointerPressed(object sender, PointerPressedEventArgs e) {
        }

        #endregion
    }

}
