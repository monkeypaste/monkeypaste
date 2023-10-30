using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using System.Windows.Input;
using Key = Avalonia.Input.Key;

namespace MonkeyPaste.Avalonia {
    public static class MpAvKeyboardCommandExtension {
        static MpAvKeyboardCommandExtension() {
            IsEnabledProperty.Changed.AddClassHandler<Control>((x, y) => HandleIsEnabledChanged(x, y));
        }

        #region Properties

        #region EnterCommand AvaloniaProperty
        public static ICommand GetEnterCommand(AvaloniaObject obj) {
            return obj.GetValue(EnterCommandProperty);
        }

        public static void SetEnterCommand(AvaloniaObject obj, ICommand value) {
            obj.SetValue(EnterCommandProperty, value);
        }

        public static readonly AttachedProperty<ICommand> EnterCommandProperty =
            AvaloniaProperty.RegisterAttached<object, Control, ICommand>(
                "EnterCommand",
                null,
                false);

        #endregion

        #region EnterCommandParameter AvaloniaProperty
        public static object GetEnterCommandParameter(AvaloniaObject obj) {
            return obj.GetValue(EnterCommandParameterProperty);
        }

        public static void SetEnterCommandParameter(AvaloniaObject obj, object value) {
            obj.SetValue(EnterCommandParameterProperty, value);
        }

        public static readonly AttachedProperty<object> EnterCommandParameterProperty =
            AvaloniaProperty.RegisterAttached<object, Control, object>(
                "EnterCommandParameter",
                null,
                false);

        #endregion

        #region SpaceCommand AvaloniaProperty
        public static ICommand GetSpaceCommand(AvaloniaObject obj) {
            return obj.GetValue(SpaceCommandProperty);
        }

        public static void SetSpaceCommand(AvaloniaObject obj, ICommand value) {
            obj.SetValue(SpaceCommandProperty, value);
        }

        public static readonly AttachedProperty<ICommand> SpaceCommandProperty =
            AvaloniaProperty.RegisterAttached<object, Control, ICommand>(
                "SpaceCommand",
                null,
                false);

        #endregion

        #region SpaceCommandParameter AvaloniaProperty
        public static object GetSpaceCommandParameter(AvaloniaObject obj) {
            return obj.GetValue(SpaceCommandParameterProperty);
        }

        public static void SetSpaceCommandParameter(AvaloniaObject obj, object value) {
            obj.SetValue(SpaceCommandParameterProperty, value);
        }

        public static readonly AttachedProperty<object> SpaceCommandParameterProperty =
            AvaloniaProperty.RegisterAttached<object, Control, object>(
                "SpaceCommandParameter",
                null,
                false);

        #endregion

        #region EscapeCommand AvaloniaProperty
        public static ICommand GetEscapeCommand(AvaloniaObject obj) {
            return obj.GetValue(EscapeCommandProperty);
        }

        public static void SetEscapeCommand(AvaloniaObject obj, ICommand value) {
            obj.SetValue(EscapeCommandProperty, value);
        }

        public static readonly AttachedProperty<ICommand> EscapeCommandProperty =
            AvaloniaProperty.RegisterAttached<object, Control, ICommand>(
                "EscapeCommand",
                null,
                false);

        #endregion

        #region EscapeCommandParameter AvaloniaProperty
        public static object GetEscapeCommandParameter(AvaloniaObject obj) {
            return obj.GetValue(EscapeCommandParameterProperty);
        }

        public static void SetEscapeCommandParameter(AvaloniaObject obj, object value) {
            obj.SetValue(EscapeCommandParameterProperty, value);
        }

        public static readonly AttachedProperty<object> EscapeCommandParameterProperty =
            AvaloniaProperty.RegisterAttached<object, Control, object>(
                "EscapeCommandParameter",
                null,
                false);

        #endregion

        #region IsEventHandled AvaloniaProperty
        public static bool GetIsEventHandled(AvaloniaObject obj) {
            return obj.GetValue(IsEventHandledProperty);
        }

        public static void SetIsEventHandled(AvaloniaObject obj, bool value) {
            obj.SetValue(IsEventHandledProperty, value);
        }

        public static readonly AttachedProperty<bool> IsEventHandledProperty =
            AvaloniaProperty.RegisterAttached<bool, Control, bool>(
                "IsEventHandled",
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
                    control.AddHandler(Control.KeyDownEvent, Control_KeyDown, RoutingStrategies.Tunnel);
                    control.Unloaded += Control_Unloaded;
                } else {
                    Control_Unloaded(control, null);
                }
            }

        }


        #endregion

        #endregion

        #region Control Event Handlers


        private static void Control_Unloaded(object sender, RoutedEventArgs e) {
            if (sender is not Control c) {
                return;
            }
            c.RemoveHandler(Control.KeyDownEvent, Control_KeyDown);
            c.Unloaded -= Control_Unloaded;
        }

        private static void Control_KeyDown(object sender, global::Avalonia.Input.KeyEventArgs e) {
            if (sender is not Control c) {
                return;
            }
            if (e.KeyModifiers != KeyModifiers.None) {
                // ignore mods
                return;
            }
            ICommand cmd = null;
            object arg = null;
            if (e.Key == Key.Enter &&
                GetEnterCommand(c) is ICommand enter_cmd) {
                cmd = enter_cmd;
                arg = GetEnterCommandParameter(c);
            } else if (e.Key == Key.Space &&
                GetSpaceCommand(c) is ICommand space_cmd) {
                cmd = space_cmd;
                arg = GetSpaceCommandParameter(c);
            } else if (e.Key == Key.Escape &&
                GetEscapeCommand(c) is ICommand escape_cmd) {
                cmd = escape_cmd;
                arg = GetEscapeCommandParameter(c);
            }
            if (cmd == null) {
                return;
            }
            cmd.Execute(arg);
            e.Handled = GetIsEventHandled(c);
        }


        #endregion
    }

}
