using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading;
using MonkeyPaste.Common.Avalonia;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MonkeyPaste.Avalonia {
    public static class MpAvLostFocusUpdateBindingExtension {
        #region Private Variables

        #endregion

        #region Constants
        #endregion
        static MpAvLostFocusUpdateBindingExtension() {
            IsEnabledProperty.Changed.AddClassHandler<Control>((x, y) => HandleIsEnabledChanged(x, y));
            TextProperty.Changed.AddClassHandler<Control>((x, y) => HandleBoundTextChanged(x, y));
        }

        #region Properties


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
                defaultValue: string.Empty,
                defaultBindingMode: BindingMode.TwoWay);
        private static void HandleBoundTextChanged(Control element, AvaloniaPropertyChangedEventArgs e) {
            if (element is TextBox tb &&
                e.NewValue is string boundText) {
                tb.SetCurrentValue(TextBox.TextProperty, boundText);
            }

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
            if (element is TextBox tb &&
                e.NewValue is bool isEnabledVal) {
                if (isEnabledVal) {
                    tb.LostFocus += TextBox_LostFocus;
                } else {
                    tb.LostFocus -= TextBox_LostFocus;
                }
            }

        }


        #endregion

        #endregion

        #region Control Event Handlers

        private static void TextBox_LostFocus(object sender, RoutedEventArgs e) {
            if (sender is TextBox tb) {
                SetText(tb, tb.Text);
            }
        }
        #endregion
    }

}
