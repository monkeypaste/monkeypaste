using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading;
using Avalonia.VisualTree;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Avalonia;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MonkeyPaste.Avalonia {
    public static class MpAvPointerCommandExtension {
        #region Private Variables

        #endregion

        #region Constants
        #endregion
        static MpAvPointerCommandExtension() {
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

        #region LeftReleaseCommand AvaloniaProperty
        public static ICommand GetLeftReleaseCommand(AvaloniaObject obj) {
            return obj.GetValue(LeftReleaseCommandProperty);
        }

        public static void SetLeftReleaseCommand(AvaloniaObject obj, ICommand value) {
            obj.SetValue(LeftReleaseCommandProperty, value);
        }

        public static readonly AttachedProperty<ICommand> LeftReleaseCommandProperty =
            AvaloniaProperty.RegisterAttached<object, Control, ICommand>(
                "LeftReleaseCommand",
                null,
                false);

        #endregion

        #region LeftReleaseCommandParameter AvaloniaProperty
        public static object GetLeftReleaseCommandParameter(AvaloniaObject obj) {
            return obj.GetValue(LeftReleaseCommandParameterProperty);
        }

        public static void SetLeftReleaseCommandParameter(AvaloniaObject obj, object value) {
            obj.SetValue(LeftReleaseCommandParameterProperty, value);
        }

        public static readonly AttachedProperty<object> LeftReleaseCommandParameterProperty =
            AvaloniaProperty.RegisterAttached<object, Control, object>(
                "LeftReleaseCommandParameter",
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

        #region RouteHoldToRightPress AvaloniaProperty
        public static bool GetRouteHoldToRightPress(AvaloniaObject obj) {
            return obj.GetValue(RouteHoldToRightPressProperty);
        }

        public static void SetRouteHoldToRightPress(AvaloniaObject obj, bool value) {
            obj.SetValue(RouteHoldToRightPressProperty, value);
        }

        public static readonly AttachedProperty<bool> RouteHoldToRightPressProperty =
            AvaloniaProperty.RegisterAttached<object, Control, bool>(
                "RouteHoldToRightPress",
                true);

        #endregion

        #region IsEventHandled AvaloniaProperty
        public static bool GetIsEventHandled(AvaloniaObject obj) {
            return obj.GetValue(IsEventHandledProperty);
        }

        public static void SetIsEventHandled(AvaloniaObject obj, bool value) {
            obj.SetValue(IsEventHandledProperty, value);
        }

        public static readonly AttachedProperty<bool> IsEventHandledProperty =
            AvaloniaProperty.RegisterAttached<object, Control, bool>(
                "IsEventHandled",
                true);

        #endregion

        #region PointerGestureDelayMs AvaloniaProperty
        public static int GetPointerGestureDelayMs(AvaloniaObject obj) {
            return obj.GetValue(PointerGestureDelayMsProperty);
        }

        public static void SetPointerGestureDelayMs(AvaloniaObject obj, int value) {
            obj.SetValue(PointerGestureDelayMsProperty, value);
        }

        public static readonly AttachedProperty<int> PointerGestureDelayMsProperty =
            AvaloniaProperty.RegisterAttached<object, Control, int>(
                "PointerGestureDelayMs",
                500,
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
            if (element is Control control &&
                e.NewValue is bool isEnabledVal) {
                if (isEnabledVal) {
                    control.AttachedToVisualTree += EnabledControl_AttachedToVisualHandler;
                    control.DetachedFromVisualTree += DisabledControl_DetachedToVisualHandler;
                    if (control.IsInitialized) {
                        EnabledControl_AttachedToVisualHandler(control, null);
                    }
                } else {
                    DisabledControl_DetachedToVisualHandler(element, null);
                }
            }

        }

        #endregion

        #endregion

        #region Control Event Handlers

        private static void EnabledControl_AttachedToVisualHandler(object s, VisualTreeAttachmentEventArgs? e) {
            if (s is Control control) {
                bool has_any_press =
                    GetLeftPressCommand(control) != null ||
                    GetRightPressCommand(control) != null ||
                    GetDoubleLeftPressCommand(control) != null;


                if (has_any_press) {
                    if (control is Button b && GetLeftPressCommand(control) != null) {
                        // NOTE pointerpress is swallowed by button unless tunneled, may need for other controls too...
                        b.AddHandler(Button.PointerPressedEvent, Control_PointerPressed, RoutingStrategies.Tunnel);
                    }
                    control.AddHandler(Control.PointerPressedEvent, Control_PointerPressed, GetRoutingStrategy(control));

                    if (GetRightPressCommand(control) is ICommand rght_press_cmd && GetRouteHoldToRightPress(control)) {
                        control.AddHandler(Control.HoldingEvent, Control_Holding, RoutingStrategies.Tunnel);
                    }
                }

                if (GetLeftReleaseCommand(control) != null) {
                    control.AddHandler(Control.PointerReleasedEvent, Control_PointerReleased, GetRoutingStrategy(control));
                }
            }
        }

        private static void DisabledControl_DetachedToVisualHandler(object s, VisualTreeAttachmentEventArgs? e) {
            if (s is Control control) {
                control.AttachedToVisualTree -= EnabledControl_AttachedToVisualHandler;
                control.DetachedFromVisualTree -= DisabledControl_DetachedToVisualHandler;
                control.PointerPressed -= Control_PointerPressed;
                control.Holding -= Control_Holding;
            }
        }

        private static void Control_PointerPressed(object sender, PointerPressedEventArgs e) {
            var control = ResolveEventControl(sender, e);
            ICommand cmd = null;
            object param = null;

            if (e.IsLeftPress(control)) {
                if (e.ClickCount == 2) {
                    cmd = GetDoubleLeftPressCommand(control);
                    param = GetDoubleLeftPressCommandParameter(control);
                } else {
                    bool can_left_press = GetLeftPressCommand(control) != null &&
                         GetLeftPressCommand(control).CanExecute(GetLeftPressCommandParameter(control));
                    bool can_left_release = GetLeftReleaseCommand(control) != null &&
                         GetLeftReleaseCommand(control).CanExecute(GetLeftReleaseCommandParameter(control));
                    bool can_double_left_press = GetDoubleLeftPressCommand(control) != null &&
                        GetDoubleLeftPressCommand(control).CanExecute(GetDoubleLeftPressCommandParameter(control));
                    bool can_hold = GetRouteHoldToRightPress(control) &&
                          GetRightPressCommand(control) != null &&
                          GetRightPressCommand(control).CanExecute(GetRightPressCommandParameter(control));

                    bool needs_double_delay_check =
                        // press vs double press
                        (can_left_press || can_left_release) && can_double_left_press;

                    bool needs_hold_check =
                          // press vs hold
                          /*(can_left_press || can_left_release) &&*/ can_hold;

                    if (needs_double_delay_check ||
                        needs_hold_check) {
                        //e.Handled = true;

                        Dispatcher.UIThread.Post(async () => {
                            bool is_still_down = true;
                            if (needs_hold_check) {
                                EventHandler<PointerReleasedEventArgs> release_handler = null;
                                is_still_down = true;
                                release_handler = (s, e) => {
                                    is_still_down = false;
                                    control.PointerReleased -= release_handler;
                                };
                                control.AddHandler(Control.PointerReleasedEvent, release_handler, RoutingStrategies.Tunnel);
                            }
                            DateTime this_press_dt = DateTime.Now;
                            bool was_new_press = false;
                            if (needs_double_delay_check) {
                                EventHandler<PointerPressedEventArgs> next_press_handler = null;
                                next_press_handler = (s, e) => {
                                    was_new_press = true;
                                    control.PointerPressed -= next_press_handler;
                                };
                                control.AddHandler(Control.PointerPressedEvent, next_press_handler, RoutingStrategies.Tunnel);
                            }
                            // to disable single vs double wait for delay if no more click
                            while (true) {
                                if (was_new_press) {
                                    // double click, reject single
                                    return;
                                }
                                if (control.DataContext is MpIDraggableViewModel dvm && dvm.IsDragging) {
                                    // drag, reject press
                                    return;
                                }
                                if (DateTime.Now - this_press_dt > TimeSpan.FromMilliseconds(GetPointerGestureDelayMs(control))) {
                                    if (needs_hold_check && is_still_down) {
                                        // hold 
                                        cmd = GetRightPressCommand(control);
                                        param = GetRightPressCommandParameter(control);
                                    } else {
                                        // single click
                                        cmd = GetLeftPressCommand(control);
                                        param = GetLeftPressCommandParameter(control);
                                    }
                                    if (cmd != null &&
                                        cmd.CanExecute(param)) {
                                        cmd.Execute(param);
                                        e.Handled = GetIsEventHandled(control);
                                    }
                                    return;
                                }
                                await Task.Delay(100);
                            }
                        });
                        return;
                    } else {
                        cmd = GetLeftPressCommand(control);
                        param = GetLeftPressCommandParameter(control);
                    }

                }
            } else if (e.IsRightPress(control)) {
                cmd = GetRightPressCommand(control);
                param = GetRightPressCommandParameter(control);
            }
            if (cmd != null && cmd.CanExecute(param)) {
                cmd.Execute(param);
                e.Handled = GetIsEventHandled(control);
            }
        }


        private static void Control_PointerReleased(object sender, PointerReleasedEventArgs e) {
            if (ResolveEventControl(sender, e) is not Control control) {
                return;
            }
            if (e.IsLeftRelease(control) &&
                GetLeftReleaseCommand(control) is ICommand cmd) {
                cmd.Execute(GetLeftReleaseCommandParameter(control));
                e.Handled = GetIsEventHandled(control);
            }
        }
        private static void Control_Holding(object sender, HoldingRoutedEventArgs e) {
            // handled manually in press evt
            e.Handled = true;
        }

        private static Control ResolveEventControl(object sender, RoutedEventArgs e) {
            var control = sender as Control;
            if (control == null && e.Source == null) {
                return null;
            }
            if (sender != e.Source &&
                e.Source is Control sc &&
                sc.GetSelfAndVisualAncestors().FirstOrDefault(x => GetIsEnabled(x)) is Control c) {
                // give source precedence (likely a child element)
                control = c;
            }
            return control;
        }


        #endregion
    }

}
