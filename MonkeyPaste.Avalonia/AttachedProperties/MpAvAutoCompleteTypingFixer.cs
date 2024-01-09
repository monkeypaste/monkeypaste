using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading;
using MonkeyPaste.Common.Avalonia;
using System;
using System.Threading.Tasks;

namespace MonkeyPaste.Avalonia {
    public static class MpAvAutoCompleteTypingFixer {
        #region Private Variables

        #endregion

        #region Statics
        static MpAvAutoCompleteTypingFixer() {
            IsEnabledProperty.Changed.AddClassHandler<Control>((x, y) => HandleIsEnabledChanged(x, y));
            TextProperty.Changed.AddClassHandler<Control>((x, y) => HandleBoundTextChanged(x, y));
        }

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
            if (e.NewValue is bool isEnabledVal &&
                    element is AutoCompleteBox acb) {
                if (isEnabledVal) {
                    acb.Loaded += Acb_Loaded;
                    acb.Unloaded += Control_Unloaded;
                    acb.TextInput += Acb_TextInput;
                    //acb.LostFocus += Acb_LostFocus;
                } else {
                    Control_Unloaded(element, null);
                }
            }
        }
        private static int? _lastCaretIdx = null;
        private static DateTime? _lastKeyDt = null;
        private static Key? _lastKey = null;
        private static void Acb_Loaded(object sender, RoutedEventArgs e) {
            if (sender is not AutoCompleteBox acb ||
                acb.GetVisualDescendant<TextBox>() is not { } tb) {
                return;
            }
            tb.LostFocus += Tb_LostFocus;
            tb.KeyDown += Tb_KeyDown;
            tb.KeyUp += Tb_KeyUp;
            tb.GetObservable(TextBox.CaretIndexProperty).Subscribe(value => OnCaretIndexChanged(tb));
        }

        private static void Tb_KeyDown(object sender, KeyEventArgs e) {
            if (sender is not TextBox tb) {
                return;
            }
            _lastKey = e.Key;
            _lastKeyDt = DateTime.Now;
        }
        private static void Tb_KeyUp(object sender, KeyEventArgs e) {
            if (sender is not TextBox tb) {
                return;
            }
            _lastKey = null;
            _lastKeyDt = null;
            _lastCaretIdx = null;
        }

        private static void OnCaretIndexChanged(TextBox tb) {
            //if(_lastCaretIdx.HasValue && )
            if (_lastKey == null ||
                _lastKey == Key.Left ||
                _lastKey == Key.Right ||
                _lastKey == Key.Up ||
                _lastKey == Key.Down ||
                _lastKey == Key.Delete ||
                _lastKey == Key.Back) {
                // na
            }
        }
        private static void Tb_LostFocus(object sender, RoutedEventArgs e) {
            if (sender is not TextBox tb ||
                MpAvFocusManager.Instance.FocusElement is not Control fc) {
                return;
            }

        }


        private static void Acb_TextInput(object sender, global::Avalonia.Input.TextInputEventArgs e) {
            if (sender is not AutoCompleteBox acb ||
                acb.Text.Length != 2 ||
                acb.GetVisualDescendant<TextBox>() is not { } tb) {
                return;
            }
            tb.CaretIndex = acb.Text.Length;
        }

        private static void Control_Unloaded(object sender, EventArgs e) {
            if (sender is not AutoCompleteBox acb) {
                return;
            }
            acb.Unloaded -= Control_Unloaded;
            acb.TextInput -= Acb_TextInput;

        }

        #endregion

        #region Text AvaloniaProperty
        public static string GetText(AvaloniaObject obj) {
            return obj.GetValue(TextProperty);
        }

        public static void SetText(AvaloniaObject obj, string value) {
            obj.SetValue(TextProperty, value);
        }

        public static readonly AttachedProperty<string> TextProperty =
            AvaloniaProperty.RegisterAttached<object, Control, string>(
                "Text",
                string.Empty,
                false,
                BindingMode.TwoWay);

        static void HandleBoundTextChanged(Control element, AvaloniaPropertyChangedEventArgs e) {
            string newText = string.Empty;
            if (e.NewValue is string) {
                newText = e.NewValue as string;
            }
            if (element is Control control) {
                if (element is TextBox tb) {
                    tb.Text = newText;
                } else if (element is AutoCompleteBox acb) {
                    acb.Text = newText;
                }
                SetLastNotifiedText(control, newText);
                SetLastNotifiedDt(control, null);
            }

        }
        #endregion

        #region DelayMs AvaloniaProperty
        public static int GetDelayMs(AvaloniaObject obj) {
            return obj.GetValue(DelayMsProperty);
        }

        public static void SetDelayMs(AvaloniaObject obj, int value) {
            obj.SetValue(DelayMsProperty, value);
        }

        public static readonly AttachedProperty<int> DelayMsProperty =
            AvaloniaProperty.RegisterAttached<object, Control, int>(
                "DelayMs",
                0);

        #endregion

        #region Private Properties

        #region LastNotifiedText AvaloniaProperty
        private static string GetLastNotifiedText(AvaloniaObject obj) {
            return obj.GetValue(LastNotifiedTextProperty);
        }

        private static void SetLastNotifiedText(AvaloniaObject obj, string value) {
            obj.SetValue(LastNotifiedTextProperty, value);
        }

        private static readonly AttachedProperty<string> LastNotifiedTextProperty =
            AvaloniaProperty.RegisterAttached<object, Control, string>(
                "LastNotifiedText",
                string.Empty,
                false,
                BindingMode.TwoWay);

        #endregion

        #region LastNotifiedDt AvaloniaProperty
        private static DateTime? GetLastNotifiedDt(AvaloniaObject obj) {
            return obj.GetValue(LastNotifiedDtProperty);
        }

        private static void SetLastNotifiedDt(AvaloniaObject obj, DateTime? value) {
            obj.SetValue(LastNotifiedDtProperty, value);
        }

        private static readonly AttachedProperty<DateTime?> LastNotifiedDtProperty =
            AvaloniaProperty.RegisterAttached<object, Control, DateTime?>(
                "LastNotifiedDt",
                null,
                false,
                BindingMode.TwoWay);

        #endregion
        #endregion

        #region Private Methods

        private static void OnTextChanged(Control control) {
            if (control == null) {
                return;
            }
            int delayMs = GetDelayMs(control);
            string lastNotifiedText = GetLastNotifiedText(control);

            if (GetLastNotifiedDt(control) == null) {
                //MpConsole.WriteLine("Input recv'd delay started");
            }

            var this_input_change_dt = DateTime.Now;
            SetLastNotifiedDt(control, this_input_change_dt);

            Dispatcher.UIThread.Post(async () => {
                while (true) {
                    var lndt = GetLastNotifiedDt(control);
                    if (lndt == null) {
                        return;
                    }
                    if (lndt != this_input_change_dt) {
                        // new input was received, cancel ntf
                        //MpConsole.WriteLine("Input recv'd update rejected");
                        return;
                    }
                    if (DateTime.Now - lndt > TimeSpan.FromMilliseconds(delayMs)) {
                        break;
                    }
                    await Task.Delay(10);
                }
                SetLastNotifiedDt(control, null);
                string controlText = null;
                if (control is TextBox tb) {
                    controlText = tb.Text;
                } else if (control is AutoCompleteBox acb) {
                    controlText = acb.Text;
                }
                if (controlText == lastNotifiedText) {
                    // ignore if text is the same
                    return;
                }
                //MpConsole.WriteLine($"Input delay reached ntf change from '{lastNotifiedText}' to '{controlText}'");
                SetLastNotifiedText(control, (string)controlText);
                SetText(control, (string)controlText);
            });
        }


        #endregion
    }
}
