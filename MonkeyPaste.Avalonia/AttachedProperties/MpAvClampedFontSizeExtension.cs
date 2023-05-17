using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.LogicalTree;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Avalonia;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace MonkeyPaste.Avalonia {
    public static class MpAvClampedFontSizeExtension {
        static MpAvClampedFontSizeExtension() {
            IsEnabledProperty.Changed.AddClassHandler<Control>((x, y) => HandleIsEnabledChanged(x, y));
            MinFontSizeProperty.Changed.AddClassHandler<Control>((x, y) => HandleMinFontSizeChanged(x, y));
            MaxFontSizeProperty.Changed.AddClassHandler<Control>((x, y) => HandleMaxFontSizeChanged(x, y));
        }
        #region Properties

        #region MinFontSize AvaloniaProperty
        public static double GetMinFontSize(AvaloniaObject obj) {
            return obj.GetValue(MinFontSizeProperty);
        }

        public static void SetMinFontSize(AvaloniaObject obj, double value) {
            obj.SetValue(MinFontSizeProperty, value);
        }

        public static readonly AttachedProperty<double> MinFontSizeProperty =
            AvaloniaProperty.RegisterAttached<object, Control, double>(
                "MinFontSize",
                1);
        private static void HandleMinFontSizeChanged(Control element, AvaloniaPropertyChangedEventArgs e) {
            OnFontSizeChanged(element);
        }
        #endregion

        #region MaxFontSize AvaloniaProperty
        public static double GetMaxFontSize(AvaloniaObject obj) {
            return obj.GetValue(MaxFontSizeProperty);
        }

        public static void SetMaxFontSize(AvaloniaObject obj, double value) {
            obj.SetValue(MaxFontSizeProperty, value);
        }

        public static readonly AttachedProperty<double> MaxFontSizeProperty =
            AvaloniaProperty.RegisterAttached<object, Control, double>(
                "MaxFontSize",
                double.MaxValue);
        private static void HandleMaxFontSizeChanged(Control element, AvaloniaPropertyChangedEventArgs e) {
            OnFontSizeChanged(element);
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
            AvaloniaProperty.RegisterAttached<object, DataGrid, bool>(
                "IsEnabled",
                false,
                false);

        private static void HandleIsEnabledChanged(Control element, AvaloniaPropertyChangedEventArgs e) {
            if (e.NewValue is bool isEnabledVal && isEnabledVal) {
                element.AttachedToVisualTree += Element_AttachedToVisualTree;
                if (element.IsInitialized) {
                    Element_AttachedToVisualTree(element, null);
                }
            } else {
                DetachedFromVisualHandler(element, null);
            }
        }

        private static void Element_AttachedToVisualTree(object sender, VisualTreeAttachmentEventArgs e) {
            var control = sender as Control;
            if (control == null) {
                return;
            }
            control.DetachedFromVisualTree += DetachedFromVisualHandler;

            control.GetObservable(TemplatedControl.FontSizeProperty).Subscribe(value => OnFontSizeChanged(control));
        }

        private static void DetachedFromVisualHandler(object s, VisualTreeAttachmentEventArgs? e) {
            var control = s as Control;
            if (control == null) {
                return;
            }
            control.AttachedToVisualTree -= Element_AttachedToVisualTree;
            control.DetachedFromVisualTree -= DetachedFromVisualHandler;
        }


        #endregion

        private static void OnFontSizeChanged(object element) {
            if (element is TemplatedControl tc &&
                Math.Clamp(tc.FontSize, GetMinFontSize(tc), GetMaxFontSize(tc)) is double clamped_font_size &&
                tc.FontSize != clamped_font_size) {
                tc.FontSize = clamped_font_size;
            }
        }

        #endregion
    }
}
