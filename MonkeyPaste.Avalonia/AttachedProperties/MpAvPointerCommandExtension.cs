using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading;
using Avalonia.VisualTree;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Avalonia;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MonkeyPaste.Avalonia {
    public class MpAvPointerCommandRules {
        public bool IsEventHandled { get; set; }
        public RoutingStrategies Routing { get; set; }
    }
    public static class MpAvPointerCommandExtension {
        #region Private Variables
        #endregion

        #region Constants
        #endregion
        static MpAvPointerCommandExtension() {
            IsEnabledProperty.Changed.AddClassHandler<Control>((x, y) => HandleIsEnabledChanged(x, y));

            LeftPressCommandProperty.Changed.AddClassHandler<Control>((x, y) => BindCommand(LeftPressCommandProperty, x));
            LeftReleaseCommandProperty.Changed.AddClassHandler<Control>((x, y) => BindCommand(LeftReleaseCommandProperty, x));
            DoubleLeftPressCommandProperty.Changed.AddClassHandler<Control>((x, y) => BindCommand(DoubleLeftPressCommandProperty, x));

            RightPressCommandProperty.Changed.AddClassHandler<Control>((x, y) => BindCommand(RightPressCommandProperty, x));
            RightReleaseCommandProperty.Changed.AddClassHandler<Control>((x, y) => BindCommand(RightReleaseCommandProperty, x));
            DoubleRightPressCommandProperty.Changed.AddClassHandler<Control>((x, y) => BindCommand(DoubleRightPressCommandProperty, x));

            HoldCommandProperty.Changed.AddClassHandler<Control>((x, y) => BindCommand(HoldCommandProperty, x));
        }

        #region Properties

        #region Pointer Commands

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
        
        #region LeftPressRules AvaloniaProperty
        public static MpAvPointerCommandRules GetLeftPressRules(AvaloniaObject obj) {
            return obj.GetValue(LeftPressRulesProperty);
        }

        public static void SetLeftPressRules(AvaloniaObject obj, MpAvPointerCommandRules value) {
            obj.SetValue(LeftPressRulesProperty, value);
        }

        public static readonly AttachedProperty<MpAvPointerCommandRules> LeftPressRulesProperty =
            AvaloniaProperty.RegisterAttached<object, Control, MpAvPointerCommandRules>(
                "LeftPressRules",
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

        #region LeftReleaseRules AvaloniaProperty
        public static MpAvPointerCommandRules GetLeftReleaseRules(AvaloniaObject obj) {
            return obj.GetValue(LeftReleaseRulesProperty);
        }

        public static void SetLeftReleaseRules(AvaloniaObject obj, MpAvPointerCommandRules value) {
            obj.SetValue(LeftReleaseRulesProperty, value);
        }

        public static readonly AttachedProperty<MpAvPointerCommandRules> LeftReleaseRulesProperty =
            AvaloniaProperty.RegisterAttached<object, Control, MpAvPointerCommandRules>(
                "LeftReleaseRules",
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

        #region DoubleLeftPressRules AvaloniaProperty
        public static MpAvPointerCommandRules GetDoubleLeftPressRules(AvaloniaObject obj) {
            return obj.GetValue(DoubleLeftPressRulesProperty);
        }

        public static void SetDoubleLeftPressRules(AvaloniaObject obj, MpAvPointerCommandRules value) {
            obj.SetValue(DoubleLeftPressRulesProperty, value);
        }

        public static readonly AttachedProperty<MpAvPointerCommandRules> DoubleLeftPressRulesProperty =
            AvaloniaProperty.RegisterAttached<object, Control, MpAvPointerCommandRules>(
                "DoubleLeftPressRules",
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

        #region RightPressRules AvaloniaProperty
        public static MpAvPointerCommandRules GetRightPressRules(AvaloniaObject obj) {
            return obj.GetValue(RightPressRulesProperty);
        }

        public static void SetRightPressRules(AvaloniaObject obj, MpAvPointerCommandRules value) {
            obj.SetValue(RightPressRulesProperty, value);
        }

        public static readonly AttachedProperty<MpAvPointerCommandRules> RightPressRulesProperty =
            AvaloniaProperty.RegisterAttached<object, Control, MpAvPointerCommandRules>(
                "RightPressRules",
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

        #region RightReleaseRules AvaloniaProperty
        public static MpAvPointerCommandRules GetRightReleaseRules(AvaloniaObject obj) {
            return obj.GetValue(RightReleaseRulesProperty);
        }

        public static void SetRightReleaseRules(AvaloniaObject obj, MpAvPointerCommandRules value) {
            obj.SetValue(RightReleaseRulesProperty, value);
        }

        public static readonly AttachedProperty<MpAvPointerCommandRules> RightReleaseRulesProperty =
            AvaloniaProperty.RegisterAttached<object, Control, MpAvPointerCommandRules>(
                "RightReleaseRules",
                null,
                false);

        #endregion

        #region DoubleRightPressCommand AvaloniaProperty
        public static ICommand GetDoubleRightPressCommand(AvaloniaObject obj) {
            return obj.GetValue(DoubleRightPressCommandProperty);
        }

        public static void SetDoubleRightPressCommand(AvaloniaObject obj, ICommand value) {
            obj.SetValue(DoubleRightPressCommandProperty, value);
        }

        public static readonly AttachedProperty<ICommand> DoubleRightPressCommandProperty =
            AvaloniaProperty.RegisterAttached<object, Control, ICommand>(
                "DoubleRightPressCommand",
                null,
                false);

        #endregion

        #region DoubleRightPressCommandParameter AvaloniaProperty
        public static object GetDoubleRightPressCommandParameter(AvaloniaObject obj) {
            return obj.GetValue(DoubleRightPressCommandParameterProperty);
        }

        public static void SetDoubleRightPressCommandParameter(AvaloniaObject obj, object value) {
            obj.SetValue(DoubleRightPressCommandParameterProperty, value);
        }

        public static readonly AttachedProperty<object> DoubleRightPressCommandParameterProperty =
            AvaloniaProperty.RegisterAttached<object, Control, object>(
                "DoubleRightPressCommandParameter",
                null,
                false);

        #endregion

        #region DoubleRightPressRules AvaloniaProperty
        public static MpAvPointerCommandRules GetDoubleRightPressRules(AvaloniaObject obj) {
            return obj.GetValue(DoubleRightPressRulesProperty);
        }

        public static void SetDoubleRightPressRules(AvaloniaObject obj, MpAvPointerCommandRules value) {
            obj.SetValue(DoubleRightPressRulesProperty, value);
        }

        public static readonly AttachedProperty<MpAvPointerCommandRules> DoubleRightPressRulesProperty =
            AvaloniaProperty.RegisterAttached<object, Control, MpAvPointerCommandRules>(
                "DoubleRightPressRules",
                null,
                false);

        #endregion

        #region HoldCommand AvaloniaProperty
        public static ICommand GetHoldCommand(AvaloniaObject obj) {
            return obj.GetValue(HoldCommandProperty);
        }

        public static void SetHoldCommand(AvaloniaObject obj, ICommand value) {
            obj.SetValue(HoldCommandProperty, value);
        }

        public static readonly AttachedProperty<ICommand> HoldCommandProperty =
            AvaloniaProperty.RegisterAttached<object, Control, ICommand>(
                "HoldCommand",
                null,
                false);

        #endregion

        #region HoldCommandParameter AvaloniaProperty
        public static object GetHoldCommandParameter(AvaloniaObject obj) {
            return obj.GetValue(HoldCommandParameterProperty);
        }

        public static void SetHoldCommandParameter(AvaloniaObject obj, object value) {
            obj.SetValue(HoldCommandParameterProperty, value);
        }

        public static readonly AttachedProperty<object> HoldCommandParameterProperty =
            AvaloniaProperty.RegisterAttached<object, Control, object>(
                "HoldCommandParameter",
                null,
                false);

        #endregion

        #region HoldRules AvaloniaProperty
        public static MpAvPointerCommandRules GetHoldRules(AvaloniaObject obj) {
            return obj.GetValue(HoldRulesProperty);
        }

        public static void SetHoldRules(AvaloniaObject obj, MpAvPointerCommandRules value) {
            obj.SetValue(HoldRulesProperty, value);
        }

        public static readonly AttachedProperty<MpAvPointerCommandRules> HoldRulesProperty =
            AvaloniaProperty.RegisterAttached<object, Control, MpAvPointerCommandRules>(
                "HoldRules",
                null,
                false);

        #endregion

        #endregion

        #region DefaultRoutingStrategy AvaloniaProperty
        public static RoutingStrategies GetDefaultRoutingStrategy(AvaloniaObject obj) {
            return obj.GetValue(DefaultRoutingStrategyProperty);
        }

        public static void SetDefaultRoutingStrategy(AvaloniaObject obj, RoutingStrategies value) {
            obj.SetValue(DefaultRoutingStrategyProperty, value);
        }

        public static readonly AttachedProperty<RoutingStrategies> DefaultRoutingStrategyProperty =
            AvaloniaProperty.RegisterAttached<object, Control, RoutingStrategies>(
                "DefaultRoutingStrategy",
                RoutingStrategies.Direct);

        #endregion

        #region RouteRightPressToHold AvaloniaProperty
        public static bool GetRouteRightPressToHold(AvaloniaObject obj) {
            return obj.GetValue(RouteRightPressToHoldProperty);
        }

        public static void SetRouteRightPressToHold(AvaloniaObject obj, bool value) {
            obj.SetValue(RouteRightPressToHoldProperty, value);
        }

        public static readonly AttachedProperty<bool> RouteRightPressToHoldProperty =
            AvaloniaProperty.RegisterAttached<object, Control, bool>(
                "RouteRightPressToHold",
                MpAvThemeViewModel.Instance.IsMobileOrWindowed ? true : false);

        #endregion

        #region DefaultIsEventHandled AvaloniaProperty
        public static bool GetDefaultIsEventHandled(AvaloniaObject obj) {
            return obj.GetValue(DefaultIsEventHandledProperty);
        }

        public static void SetDefaultIsEventHandled(AvaloniaObject obj, bool value) {
            obj.SetValue(DefaultIsEventHandledProperty, value);
        }

        public static readonly AttachedProperty<bool> DefaultIsEventHandledProperty =
            AvaloniaProperty.RegisterAttached<object, Control, bool>(
                "DefaultIsEventHandled",
                true);

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
                    control.Unloaded += Control_Unloaded;
                } else {
                    Control_Unloaded(element, null);
                }
            }

        }


        #endregion

        #endregion

        #region Private Methods

        private static void BindCommand(AvaloniaProperty cmdProp, Control cmdControl) {
            var info = GetCmdInfo(cmdProp, cmdControl);
            cmdControl.AddHandler(info.evt, info.handler, info.rules.Routing);
            if(cmdProp == RightPressCommandProperty && GetRouteRightPressToHold(cmdControl)) {                
                if(GetHoldRules(cmdControl) is { } hold_rules) {
                    cmdControl.AddHandler(Control.HoldingEvent, HoldHandler, hold_rules.Routing);
                } else {
                    cmdControl.AddHandler(Control.HoldingEvent, HoldHandler, info.rules.Routing);
                }
            }
        }

        private static (RoutedEvent evt, Delegate handler, MpAvPointerCommandRules rules) GetCmdInfo(AvaloniaProperty cmdProp, Control cmdControl) {
            MpAvPointerCommandRules rules = null;
            RoutedEvent re = null;
            Delegate handler = null;
            if (cmdProp == LeftPressCommandProperty) {
                rules = GetLeftPressRules(cmdControl);
                re = Control.PointerPressedEvent;
                handler = LeftPressedHandler;
            } else if (cmdProp == LeftReleaseCommandProperty) {
                rules = GetLeftReleaseRules(cmdControl);
                re = Control.PointerReleasedEvent;
                handler = LeftReleaseHandler;
            } else if (cmdProp == DoubleLeftPressCommandProperty) {
                rules = GetDoubleLeftPressRules(cmdControl);
                re = Control.PointerPressedEvent;
                handler = DoubleLeftPressedHandler;
            } else if (cmdProp == RightPressCommandProperty) {
                rules = GetRightPressRules(cmdControl);
                re = Control.PointerPressedEvent;
                handler = RightPressedHandler;
            } else if (cmdProp == RightReleaseCommandProperty) {
                rules = GetRightReleaseRules(cmdControl);
                re = Control.PointerReleasedEvent;
                handler = RightReleaseHandler;
            } else if (cmdProp == DoubleRightPressCommandProperty) {
                rules = GetDoubleRightPressRules(cmdControl);
                re = Control.PointerPressedEvent;
                handler = DoubleRightPressedHandler;
            } else if (cmdProp == HoldCommandProperty) {
                rules = GetHoldRules(cmdControl);
                re = Control.HoldingEvent;
                handler = HoldHandler;
            }
            if(rules == null) {
                rules = new MpAvPointerCommandRules() {
                    IsEventHandled = GetDefaultIsEventHandled(cmdControl),
                    Routing = GetDefaultRoutingStrategy(cmdControl)
                };
            }
            return (re, handler, rules);
        }

        #region Pointer Event Handlers
        private static void HoldHandler(object sender, HoldingRoutedEventArgs e) {
            if (sender is not Control c ||
                GetHoldCommand(c) is not { } cmd) {
                return;
            }
            object param = GetHoldCommandParameter(c);
            if (!cmd.CanExecute(param)) {
                return;
            }
            cmd.Execute(param);
            e.Handled = GetCmdInfo(HoldCommandProperty, c).rules.IsEventHandled;
        }

        private static void DoubleLeftPressedHandler(object sender, PointerPressedEventArgs e) {
            if (e.ClickCount != 2 ||
                sender is not Control c ||
                !e.IsLeftPress(c) ||
                GetDoubleLeftPressCommand(c) is not { } cmd) {
                return;
            }
            object param = GetDoubleLeftPressCommandParameter(c);
            if (!cmd.CanExecute(param)) {
                return;
            }
            cmd.Execute(param);
            e.Handled = GetCmdInfo(LeftPressCommandProperty, c).rules.IsEventHandled;
        }

        private static void LeftReleaseHandler(object sender, PointerReleasedEventArgs e) {
            if (sender is not Control c ||
                !e.IsLeftRelease(c) ||
                GetLeftReleaseCommand(c) is not { } cmd) {
                return;
            }
            object param = GetLeftReleaseCommandParameter(c);
            if (!cmd.CanExecute(param)) {
                return;
            }
            cmd.Execute(param);
            e.Handled = GetCmdInfo(LeftReleaseCommandProperty, c).rules.IsEventHandled;
        }

        private static void LeftPressedHandler(object sender, PointerPressedEventArgs e) {
            if (sender is not Control c ||
                !e.IsLeftPress(c) ||
                GetLeftPressCommand(c) is not { } cmd) {
                return;
            }
            object param = GetLeftPressCommandParameter(c);
            if (!cmd.CanExecute(param)) {
                return;
            }
            cmd.Execute(param);
            e.Handled = GetCmdInfo(LeftPressCommandProperty, c).rules.IsEventHandled;
        }

        private static void DoubleRightPressedHandler(object sender, PointerPressedEventArgs e) {
            if (e.ClickCount != 2 ||
                sender is not Control c || 
                !e.IsRightPress(c) ||
                GetDoubleRightPressCommand(c) is not { } cmd) {
                return;
            }
            object param = GetDoubleRightPressCommandParameter(c);
            if (!cmd.CanExecute(param)) {
                return;
            }
            cmd.Execute(param);
            e.Handled = GetCmdInfo(DoubleRightPressCommandProperty, c).rules.IsEventHandled;
        }

        private static void RightReleaseHandler(object sender, PointerReleasedEventArgs e) {
            if (sender is not Control c ||
                !e.IsRightRelease(c) ||
                GetRightReleaseCommand(c) is not { } cmd) {
                return;
            }
            object param = GetRightReleaseCommandParameter(c);
            if (!cmd.CanExecute(param)) {
                return;
            }
            cmd.Execute(param);
            e.Handled = GetCmdInfo(RightReleaseCommandProperty, c).rules.IsEventHandled;
        }

        private static void RightPressedHandler(object sender, PointerPressedEventArgs e) {
            if (sender is not Control c ||
                !e.IsRightPress(c) ||
                GetRightPressCommand(c) is not { } cmd) {
                return;
            }
            object param = GetRightPressCommandParameter(c);
            if (!cmd.CanExecute(param)) {
                return;
            }
            cmd.Execute(param);
            e.Handled = GetCmdInfo(RightPressCommandProperty, c).rules.IsEventHandled;
        }
        #endregion

        private static void Control_Unloaded(object sender, RoutedEventArgs e) {
            if (sender is not Control c) {
                return;
            }
            c.Unloaded -= Control_Unloaded;
            c.PointerPressed -= LeftPressedHandler;
            c.PointerPressed -= RightPressedHandler;
            c.PointerPressed -= DoubleLeftPressedHandler;
            c.PointerPressed -= DoubleRightPressedHandler;
            
            c.PointerReleased -= LeftReleaseHandler;
            c.PointerReleased -= RightReleaseHandler;

            c.Holding -= HoldHandler;
        }

        #endregion
    }

}
