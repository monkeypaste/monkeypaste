using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Avalonia;
using System;
using System.Diagnostics;
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

        #region ButtonClickCommand AvaloniaProperty
        public static ICommand GetButtonClickCommand(AvaloniaObject obj) {
            return obj.GetValue(ButtonClickCommandProperty);
        }

        public static void SetButtonClickCommand(AvaloniaObject obj, ICommand value) {
            obj.SetValue(ButtonClickCommandProperty, value);
        }

        public static readonly AttachedProperty<ICommand> ButtonClickCommandProperty =
            AvaloniaProperty.RegisterAttached<object, Control, ICommand>(
                "ButtonClickCommand",
                null,
                false);

        #endregion

        #region ButtonClickCommandParameter AvaloniaProperty
        public static object GetButtonClickCommandParameter(AvaloniaObject obj) {
            return obj.GetValue(ButtonClickCommandParameterProperty);
        }

        public static void SetButtonClickCommandParameter(AvaloniaObject obj, object value) {
            obj.SetValue(ButtonClickCommandParameterProperty, value);
        }

        public static readonly AttachedProperty<object> ButtonClickCommandParameterProperty =
            AvaloniaProperty.RegisterAttached<object, Control, object>(
                "ButtonClickCommandParameter",
                null,
                false);

        #endregion

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

        #region HoldingCommand AvaloniaProperty
        public static ICommand GetHoldingCommand(AvaloniaObject obj) {
            return obj.GetValue(HoldingCommandProperty);
        }

        public static void SetHoldingCommand(AvaloniaObject obj, ICommand value) {
            obj.SetValue(HoldingCommandProperty, value);
        }

        public static readonly AttachedProperty<ICommand> HoldingCommandProperty =
            AvaloniaProperty.RegisterAttached<object, Control, ICommand>(
                "HoldingCommand",
                null,
                false);

        #endregion

        #region HoldingCommandParameter AvaloniaProperty
        public static object GetHoldingCommandParameter(AvaloniaObject obj) {
            return obj.GetValue(HoldingCommandParameterProperty);
        }

        public static void SetHoldingCommandParameter(AvaloniaObject obj, object value) {
            obj.SetValue(HoldingCommandParameterProperty, value);
        }

        public static readonly AttachedProperty<object> HoldingCommandParameterProperty =
            AvaloniaProperty.RegisterAttached<object, Control, object>(
                "HoldingCommandParameter",
                null,
                false);

        #endregion

        #region DragEnterCommand AvaloniaProperty
        public static ICommand GetDragEnterCommand(AvaloniaObject obj) {
            return obj.GetValue(DragEnterCommandProperty);
        }

        public static void SetDragEnterCommand(AvaloniaObject obj, ICommand value) {
            obj.SetValue(DragEnterCommandProperty, value);
        }

        public static readonly AttachedProperty<ICommand> DragEnterCommandProperty =
            AvaloniaProperty.RegisterAttached<object, Control, ICommand>(
                "DragEnterCommand",
                null,
                false);

        #endregion

        #region DragEnterCommandParameter AvaloniaProperty
        public static object GetDragEnterCommandParameter(AvaloniaObject obj) {
            return obj.GetValue(DragEnterCommandParameterProperty);
        }

        public static void SetDragEnterCommandParameter(AvaloniaObject obj, object value) {
            obj.SetValue(DragEnterCommandParameterProperty, value);
        }

        public static readonly AttachedProperty<object> DragEnterCommandParameterProperty =
            AvaloniaProperty.RegisterAttached<object, Control, object>(
                "DragEnterCommandParameter",
                null,
                false);

        #endregion

        #region IsHoldingEnabled AvaloniaProperty
        public static bool GetIsHoldingEnabled(AvaloniaObject obj) {
            return obj.GetValue(IsHoldingEnabledProperty);
        }

        public static void SetIsHoldingEnabled(AvaloniaObject obj, bool value) {
            obj.SetValue(IsHoldingEnabledProperty, value);
        }

        public static readonly AttachedProperty<bool> IsHoldingEnabledProperty =
            AvaloniaProperty.RegisterAttached<object, Control, bool>(
                "IsHoldingEnabled",
                true);

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
                    GetDoubleLeftPressCommand(control) != null ||
                    (GetHoldingCommand(control) != null && GetIsHoldingEnabled(control));


                if (has_any_press) {
                    if (control is Button b && GetLeftPressCommand(control) != null) {
                        // NOTE pointerpress is swallowed by button unless tunneled, may need for other controls too...
                        b.AddHandler(Button.PointerPressedEvent, Control_PointerPressed, RoutingStrategies.Tunnel);
                    }
                    control.AddHandler(Control.PointerPressedEvent, Control_PointerPressed, GetRoutingStrategy(control));

                    if (GetHoldingCommand(control) != null &&
                        GetIsHoldingEnabled(control)) {
                        control.AddHandler(Control.HoldingEvent, Control_Holding, RoutingStrategies.Tunnel);
                    }
                }

                if (GetLeftReleaseCommand(control) != null) {
                    control.AddHandler(Control.PointerReleasedEvent, Control_PointerReleased, GetRoutingStrategy(control));
                }

                if (GetButtonClickCommand(control) != null) {
                    if (control is Button button) {
                        button.AddHandler(Button.ClickEvent, Button_Click, GetRoutingStrategy(control));
                    } else {
                        MpDebug.Break("Error must be attached to button");
                    }
                }

                if (GetDragEnterCommand(control) != null) {
                    EnableDragEnter(control);
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
                    bool can_hold = GetIsHoldingEnabled(control) &&
                          GetHoldingCommand(control) != null &&
                          GetHoldingCommand(control).CanExecute(GetHoldingCommandParameter(control));

                    bool needs_double_delay_check =
                        // press vs double press
                        (can_left_press || can_left_release) && can_double_left_press;

                    bool needs_hold_check =
                          // press vs hold
                          (can_left_press || can_left_release) && can_hold;

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
                                control.PointerReleased += release_handler;
                            }
                            DateTime this_press_dt = DateTime.Now;
                            bool was_new_press = false;
                            if (needs_double_delay_check) {
                                EventHandler<PointerPressedEventArgs> next_press_handler = null;
                                next_press_handler = (s, e) => {
                                    was_new_press = false;
                                    control.PointerPressed -= next_press_handler;
                                };
                                control.PointerPressed += next_press_handler;
                            }
                            // to disable single vs double wait for delay if no more click
                            while (true) {
                                if (was_new_press) {
                                    // double click, reject single
                                    return;
                                }
                                if (DateTime.Now - this_press_dt > TimeSpan.FromMilliseconds(GetPointerGestureDelayMs(control))) {
                                    if (needs_hold_check && is_still_down) {
                                        // hold 
                                        cmd = GetHoldingCommand(control);
                                        param = GetHoldingCommandParameter(control);
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


        private static void Button_Click(object sender, RoutedEventArgs e) {
            throw new NotImplementedException();
        }

        private static void Control_PointerReleased(object sender, PointerReleasedEventArgs e) {

            var control = ResolveEventControl(sender, e);
            if (control == null) {
                return;
            }
            if (e.IsLeftRelease(control) &&
                GetLeftReleaseCommand(control) is ICommand cmd) {
                cmd.Execute(GetLeftReleaseCommandParameter(control));
                e.Handled = GetIsEventHandled(control);
            }
        }
        private static void Control_Holding(object sender, HoldingRoutedEventArgs e) {
            // handled manually in press
            e.Handled = true;
        }

        private static Control ResolveEventControl(object sender, PointerEventArgs e) {
            var control = sender as Control;
            if (control == null && e.Source == null) {
                return null;
            }
            if (sender != e.Source &&
                e.Source is Control c &&
                GetIsEnabled(c)) {
                // give source precedence (likely a child element)
                control = c;
            }
            return control;
        }

        #region Dnd

        private static void EnableDragEnter(Control control) {
            void Control_DragEnter(object sender, DragEventArgs e) {
                if (GetDragEnterCommand(control) is ICommand cmd) {
                    cmd.Execute(GetDragEnterCommandParameter(control));
                }
            }
            DragDrop.SetAllowDrop(control, true);
            control.AddHandler(DragDrop.DragEnterEvent, Control_DragEnter);
        }

        #endregion

        #endregion
    }

}
