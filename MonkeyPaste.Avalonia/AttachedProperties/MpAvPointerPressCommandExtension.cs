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
using MonkeyPaste.Common;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using Avalonia.Threading;
using Avalonia.Controls.Primitives;
using System.Diagnostics;
using System.Threading.Tasks;

namespace MonkeyPaste.Avalonia {
    public static class MpAvPointerPressCommandExtension {
        #region Private Variables

        #endregion

        #region Constants
        #endregion
        static MpAvPointerPressCommandExtension() {
            IsEnabledProperty.Changed.AddClassHandler<Control>((x, y) => HandleIsEnabledChanged(x, y));
        }

        #region Properties

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

        #region RoutingStrategy AvaloniaProperty
        public static RoutingStrategies GetRoutingStrategy(AvaloniaObject obj) {
            return obj.GetValue(RoutingStrategyProperty);
        }

        public static void SetRoutingStrategy(AvaloniaObject obj, RoutingStrategies value) {
            obj.SetValue(RoutingStrategyProperty, value);
        }

        public static readonly AttachedProperty<RoutingStrategies> RoutingStrategyProperty =
            AvaloniaProperty.RegisterAttached<object, Control, RoutingStrategies>(
                "RoutingStrategy",
                RoutingStrategies.Direct);

        #endregion

        #region IsPressEventHandled AvaloniaProperty
        public static bool GetIsPressEventHandled(AvaloniaObject obj) {
            return obj.GetValue(IsPressEventHandledProperty);
        }

        public static void SetIsPressEventHandled(AvaloniaObject obj, bool value) {
            obj.SetValue(IsPressEventHandledProperty, value);
        }

        public static readonly AttachedProperty<bool> IsPressEventHandledProperty =
            AvaloniaProperty.RegisterAttached<object, Control, bool>(
                "IsPressEventHandled",
                true,
                false);

        #endregion
        
        #region DoubleClickDelayMs AvaloniaProperty
        public static int GetDoubleClickDelayMs(AvaloniaObject obj) {
            return obj.GetValue(DoubleClickDelayMsProperty);
        }

        public static void SetDoubleClickDelayMs(AvaloniaObject obj, int value) {
            obj.SetValue(DoubleClickDelayMsProperty, value);
        }

        public static readonly AttachedProperty<int> DoubleClickDelayMsProperty =
            AvaloniaProperty.RegisterAttached<object, Control, int>(
                "DoubleClickDelayMs",
                300,
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
                    control.AttachedToVisualTree += EnabledControl_AttachedToVisualHandler;
                    control.DetachedFromVisualTree += DisabledControl_DetachedToVisualHandler;
                    if (control.IsInitialized) {
                        EnabledControl_AttachedToVisualHandler(control, null);
                    } 
                }
            } else {
                DisabledControl_DetachedToVisualHandler(element, null);
            }
        }

        #endregion

        #endregion

        #region Control Event Handlers

        private static void EnabledControl_AttachedToVisualHandler(object s, VisualTreeAttachmentEventArgs? e) {
            if (s is Control control) {
                if (control is Button b && GetLeftPressCommand(control) is ICommand leftPressCommand) {
                    // NOTE pointerpress is swallowed by button unless tunneled, may need for other controls too...
                    b.AddHandler(Button.PointerPressedEvent, Control_PointerPressed, RoutingStrategies.Tunnel);
                }
                control.AddHandler(Button.PointerPressedEvent, Control_PointerPressed, GetRoutingStrategy(control));
            }
        }


        private static void DisabledControl_DetachedToVisualHandler(object s, VisualTreeAttachmentEventArgs? e) {
            if (s is Control control) {
                // control
                control.AttachedToVisualTree -= EnabledControl_AttachedToVisualHandler;
                control.DetachedFromVisualTree -= DisabledControl_DetachedToVisualHandler;
                control.PointerPressed -= Control_PointerPressed;
            }
        }
        private static DateTime? _lastClickDateTime = null;
        private static void Control_PointerPressed(object sender, PointerPressedEventArgs e) {
            _lastClickDateTime = DateTime.Now;
            if(sender is Control control) {
                ICommand cmd = null;
                object param = null;
                if(e.IsLeftPress(control)) {
                    if(e.ClickCount == 2) {
                        cmd = GetDoubleLeftPressCommand(control);
                        param = GetDoubleLeftPressCommandParameter(control);
                    } else {

                        if (GetLeftPressCommand(control) != null &&
                            GetDoubleLeftPressCommand(control) != null &&
                            GetLeftPressCommand(control).CanExecute(GetLeftPressCommandParameter(control)) && 
                            GetDoubleLeftPressCommand(control).CanExecute(GetDoubleLeftPressCommandParameter(control))) {
                            Dispatcher.UIThread.Post(async () => {
                                // to disable single vs double wait for delay if no more click
                                var ct = _lastClickDateTime;
                                _lastClickDateTime = null;
                                while (true) {
                                    if(_lastClickDateTime != null) {
                                        // double click, reject single
                                        return;
                                    }
                                    if (DateTime.Now - ct > TimeSpan.FromMilliseconds(GetDoubleClickDelayMs(control))) {
                                        // single click
                                        cmd = GetLeftPressCommand(control);
                                        param = GetLeftPressCommandParameter(control);
                                        if (cmd.CanExecute(param)) {
                                            cmd.Execute(param);
                                            e.Handled = GetIsPressEventHandled(control);
                                        }
                                        return;
                                    }
                                    await Task.Delay(100);
                                }
                            });
                            return;
                        }else {
                            cmd = GetLeftPressCommand(control);
                            param = GetLeftPressCommandParameter(control);
                        }
                        
                    }
                } else if(e.IsRightPress(control)) {
                    cmd = GetRightPressCommand(control);
                    param = GetRightPressCommandParameter(control);
                }
                if (cmd != null && cmd.CanExecute(param)) {
                   cmd.Execute(param);
                   e.Handled = GetIsPressEventHandled(control);
                }
            }
        }

        #endregion
    }

}
