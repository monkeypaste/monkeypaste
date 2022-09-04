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
using MonkeyPaste.Common;

namespace MonkeyPaste.Avalonia {
    public static class MpAvMousePressCommand {
        #region Private Variables

        private static MpPoint _mouseLeftDownPosition;
        private static object _mouseDownObject;

        #endregion

        #region Constants

        public const double MIN_DRAG_DIST = 5;
        #endregion

        static MpAvMousePressCommand() {
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

        #region DoubleLeftPressCommandParameter AvaloniaProperty
        public static object GetDoubleLeftPressCommandParameter(AvaloniaObject obj) {
            return obj.GetValue(DoubleLeftPressCommandParameterProperty);
        }

        public static void SetDoubleLeftPressCommandParameter(AvaloniaObject obj, object value) {
            obj.SetValue(DoubleLeftPressCommandParameterProperty, value);
        }

        public static readonly AttachedProperty<object> DoubleLeftPressCommandParameterProperty =
            AvaloniaProperty.RegisterAttached<object, Control, object>(
                "DoubleLeftPressCommandParameter",
                null,
                false);

        #endregion

        #region LeftDragCommand AvaloniaProperty
        public static ICommand GetLeftDragBeginCommand(AvaloniaObject obj) {
            return obj.GetValue(LeftDragBeginCommandProperty);
        }

        public static void SetLeftDragBeginCommand(AvaloniaObject obj, ICommand value) {
            obj.SetValue(LeftDragBeginCommandProperty, value);
        }

        public static readonly AttachedProperty<ICommand> LeftDragBeginCommandProperty =
            AvaloniaProperty.RegisterAttached<object, Control, ICommand>(
                "LeftDragBeginCommand",
                null,
                false);

        #endregion

        #region LeftDragCommandParameter AvaloniaProperty
        public static object GetLeftDragCommandParameter(AvaloniaObject obj) {
            return obj.GetValue(LeftDragCommandParameterProperty);
        }

        public static void SetLeftDragCommandParameter(AvaloniaObject obj, object value) {
            obj.SetValue(LeftDragCommandParameterProperty, value);
        }

        public static readonly AttachedProperty<object> LeftDragCommandParameterProperty =
            AvaloniaProperty.RegisterAttached<object, Control, object>(
                "LeftDragCommandParameter",
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
                
                if(GetLeftDragBeginCommand(control) != null) {
                    control.PointerMoved += Control_PointerMoved;
                    control.PointerReleased += Control_PointerReleased;
                }
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
            if(sender is Control control) {
                ICommand cmd = null;
                object param = null;
                if(e.IsLeftPress(control)) {
                    if(GetLeftDragBeginCommand(control) != null) {
                        _mouseLeftDownPosition = e.GetClientMousePoint(control);
                    } else {
                        // null'd in rele
                        _mouseLeftDownPosition = null;
                    }

                    if(e.ClickCount == 2) {
                        cmd = GetDoubleLeftPressCommand(control);
                        param = GetDoubleLeftPressCommandParameter(control);
                    } else {
                        cmd = GetLeftPressCommand(control);
                        param = GetLeftPressCommandParameter(control);
                    }
                } else if(e.IsRightPress(control)) {
                    cmd = GetRightPressCommand(control);
                    param = GetRightPressCommandParameter(control);
                }
                if (cmd != null) {
                   cmd.Execute(param);
                }
            }
        }


        private static void Control_PointerMoved(object sender, PointerEventArgs e) {
           // NOTE only added when DragBeginCommand exists
           if(sender is Control control && 
                GetLeftDragBeginCommand(control) is ICommand dragBeginCommand &&
                e.IsLeftDown(control) &&
                _mouseLeftDownPosition != null) {

                var drag_dist = _mouseLeftDownPosition.Distance(e.GetClientMousePoint(control));
                if(drag_dist >= MpAvShortcutCollectionViewModel.MIN_GLOBAL_DRAG_DIST) {
                    dragBeginCommand.Execute(GetLeftDragCommandParameter(control));
                }

            }
        }


        private static void Control_PointerReleased(object sender, PointerReleasedEventArgs e) {
            _mouseLeftDownPosition = null;
        }
        #endregion
    }

}
