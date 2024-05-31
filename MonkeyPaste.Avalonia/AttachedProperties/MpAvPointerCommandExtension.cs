using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading;
using Avalonia.VisualTree;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Avalonia;
using System;
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

        #region DoubleLeftReleaseCommand AvaloniaProperty
        public static ICommand GetDoubleLeftReleaseCommand(AvaloniaObject obj) {
            return obj.GetValue(DoubleLeftReleaseCommandProperty);
        }

        public static void SetDoubleLeftReleaseCommand(AvaloniaObject obj, ICommand value) {
            obj.SetValue(DoubleLeftReleaseCommandProperty, value);
        }

        public static readonly AttachedProperty<ICommand> DoubleLeftReleaseCommandProperty =
            AvaloniaProperty.RegisterAttached<object, Control, ICommand>(
                "DoubleLeftReleaseCommand",
                null,
                false);

        #endregion

        #region DoubleLeftReleaseCommandParameter AvaloniaProperty
        public static object GetDoubleLeftReleaseCommandParameter(AvaloniaObject obj) {
            return obj.GetValue(DoubleLeftReleaseCommandParameterProperty);
        }

        public static void SetDoubleLeftReleaseCommandParameter(AvaloniaObject obj, object value) {
            obj.SetValue(DoubleLeftReleaseCommandParameterProperty, value);
        }

        public static readonly AttachedProperty<object> DoubleLeftReleaseCommandParameterProperty =
            AvaloniaProperty.RegisterAttached<object, Control, object>(
                "DoubleLeftReleaseCommandParameter",
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

        #region RightReleaseCommand AvaloniaProperty
        public static ICommand GetRightReleaseCommand(AvaloniaObject obj) {
            return obj.GetValue(RightReleaseCommandProperty);
        }

        public static void SetRightReleaseCommand(AvaloniaObject obj, ICommand value) {
            obj.SetValue(RightReleaseCommandProperty, value);
        }

        public static readonly AttachedProperty<ICommand> RightReleaseCommandProperty =
            AvaloniaProperty.RegisterAttached<object, Control, ICommand>(
                "RightReleaseCommand",
                null,
                false);

        #endregion

        #region RightReleaseCommandParameter AvaloniaProperty
        public static object GetRightReleaseCommandParameter(AvaloniaObject obj) {
            return obj.GetValue(RightReleaseCommandParameterProperty);
        }

        public static void SetRightReleaseCommandParameter(AvaloniaObject obj, object value) {
            obj.SetValue(RightReleaseCommandParameterProperty, value);
        }

        public static readonly AttachedProperty<object> RightReleaseCommandParameterProperty =
            AvaloniaProperty.RegisterAttached<object, Control, object>(
                "RightReleaseCommandParameter",
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
#if MOBILE_OR_WINDOWED
                true);
#else
                false);
#endif

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
                bool press_added = false;
                bool has_any_press =
                    GetLeftPressCommand(control) != null ||
                    GetRightPressCommand(control) != null ||
                    GetDoubleLeftPressCommand(control) != null ||
                    GetDoubleLeftReleaseCommandParameter(control) != null;

                if (has_any_press) {
                    if (control is Button b && GetLeftPressCommand(control) != null) {
                        // NOTE pointerpress is swallowed by button unless tunneled, may need for other controls too...
                        b.AddHandler(Button.PointerPressedEvent, Control_PointerPressed, RoutingStrategies.Tunnel);
                    }
                    control.AddHandler(Control.PointerPressedEvent, Control_PointerPressed, GetRoutingStrategy(control));
                    press_added = true;
                    
                }

                if (GetLeftReleaseCommand(control) != null || GetRightReleaseCommand(control) != null) {
                    control.AddHandler(Control.PointerReleasedEvent, Control_PointerReleased, GetRoutingStrategy(control));
                }

                if (GetRouteHoldToRightPress(control) &&
                    (GetRightPressCommand(control) is not null || GetRightReleaseCommand(control) is not null)) {
                    control.AddHandler(Control.HoldingEvent, Control_Holding, RoutingStrategies.Tunnel);
                    if(!press_added) {
                        control.AddHandler(Control.PointerPressedEvent, Control_PointerPressed, GetRoutingStrategy(control));
                    }
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
                    bool can_double_left_release = GetDoubleLeftReleaseCommand(control) != null &&
                        GetDoubleLeftReleaseCommand(control).CanExecute(GetDoubleLeftReleaseCommandParameter(control));
                    bool can_hold = GetRouteHoldToRightPress(control) &&
                          GetRightPressCommand(control) != null &&
                          GetRightPressCommand(control).CanExecute(GetRightPressCommandParameter(control));

                    bool needs_double_delay_check =
                        // press vs double press
                        (can_left_press || can_left_release) && can_double_left_press;
                        //can_double_left_press || can_double_left_release;

                    bool needs_hold_check =
                          // press vs hold
                          /*(can_left_press || can_left_release) &&*/ can_hold;

                    if (needs_double_delay_check ||
                        needs_hold_check) {
                        //e.Handled = true;

                        Dispatcher.UIThread.Post(async () => {
                            bool is_still_down = true;
                            double drag_dist = 0;
                            if (needs_hold_check) {
                                MpPoint down_mp = e.GetPosition(control).ToPortablePoint();
                                void move_handler(object s, PointerEventArgs e) {
                                    if (!e.IsLeftDown(control)) {
                                        is_still_down = false;
                                        control.RemoveHandler(Control.PointerMovedEvent, move_handler);
                                        return;
                                    }
                                    drag_dist = e.GetPosition(control).ToPortablePoint().Distance(down_mp);
                                }
                                void release_handler(object s, PointerReleasedEventArgs e) {
                                    is_still_down = false;
                                    control.RemoveHandler(Control.PointerReleasedEvent, release_handler);
                                    control.RemoveHandler(Control.PointerMovedEvent, move_handler);
                                }
                                control.AddHandler(Control.PointerMovedEvent, move_handler, RoutingStrategies.Tunnel);
                                control.AddHandler(Control.PointerReleasedEvent, release_handler, RoutingStrategies.Tunnel);
                            }
                            DateTime this_press_dt = DateTime.Now;
                            bool was_new_press = false;
                            if (needs_double_delay_check) {
                                EventHandler<PointerPressedEventArgs> next_press_handler = null;
                                next_press_handler = (s, e) => {
                                    was_new_press = true;
                                    control.RemoveHandler(Control.PointerPressedEvent, next_press_handler);
                                };
                                control.AddHandler(Control.PointerPressedEvent, next_press_handler, RoutingStrategies.Tunnel);
                            }
                            // to disable single vs double wait for delay if no more click
                            while (true) {
                                if (was_new_press) {
                                    // double click, reject single
                                    return;
                                }
                                if (control.DataContext is MpIDraggable dvm && dvm.IsDragging) {
                                    // drag, reject press
                                    return;
                                }
                                if (DateTime.Now - this_press_dt > TimeSpan.FromMilliseconds(GetPointerGestureDelayMs(control))) {
                                    if (needs_hold_check &&
                                        is_still_down &&
                                        drag_dist < 5) {
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
                return;
            }
            if (e.IsRightRelease(control) &&
                GetRightReleaseCommand(control) is ICommand cmd2) {
                cmd2.Execute(GetRightReleaseCommandParameter(control));
                e.Handled = GetIsEventHandled(control);
                return;
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
#if MAC
            // BUG (this could a debug thing from pause/resume but) for some reason when press event shows a dialog on mac the source button captures the pointer
            // and after the dialog closes it just keeps thinking the pointer is over that source control so 
            // trying to verify by position here
            //if (e is PointerPressedEventArgs ppe && ppe.GetPosition(control) is { } control_mp &&
            //        !control.Bounds.Contains(control_mp)) {
            //    control = null;
            //} 
#endif
            return control;
        }


        #endregion
    }

}
