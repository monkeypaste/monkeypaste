using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using MonkeyPaste;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Wpf;

namespace MpWpfApp {
    public class MpGradientBackgroundExtension : DependencyObject {
        #region BaseColor

        public static Brush GetDefaultBackgroundBrush(DependencyObject obj) {
            return (Brush)obj.GetValue(DefaultBackgroundBrushProperty);
        }
        public static void SetDefaultBackgroundBrush(DependencyObject obj, Brush value) {
            obj.SetValue(DefaultBackgroundBrushProperty, value);
        }
        public static readonly DependencyProperty DefaultBackgroundBrushProperty =
          DependencyProperty.RegisterAttached(
            "DefaultBackgroundBrush",
            typeof(Brush),
            typeof(MpGradientBackgroundExtension),
            new FrameworkPropertyMetadata(Brushes.Green));

        #endregion

        #region Angle

        public static double GetAngle(DependencyObject obj) {
            return (double)obj.GetValue(AngleProperty);
        }
        public static void SetAngle(DependencyObject obj, double value) {
            obj.SetValue(AngleProperty, value);
        }
        public static readonly DependencyProperty AngleProperty =
          DependencyProperty.RegisterAttached(
            "Angle",
            typeof(double),
            typeof(MpGradientBackgroundExtension),
            new FrameworkPropertyMetadata(0.0));

        #endregion

        #region IsEnabled

        public static bool GetIsEnabled(DependencyObject obj) {
            return (bool)obj.GetValue(IsEnabledProperty);
        }
        public static void SetIsEnabled(DependencyObject obj, bool value) {
            obj.SetValue(IsEnabledProperty, value);
        }
        public static readonly DependencyProperty IsEnabledProperty =
          DependencyProperty.RegisterAttached(
            "IsEnabled",
            typeof(bool),
            typeof(MpGradientBackgroundExtension),
            new FrameworkPropertyMetadata {
                PropertyChangedCallback = (obj, e) => {
                    var fe = obj as FrameworkElement;
                    if (fe == null) {
                        return;
                    }

                    if ((bool)e.NewValue == true) {
                        SetBackground(fe);
                    } else {
                        Fe_Unloaded(fe, null);
                    }
                }
            });

        #endregion

        private static void SetBackground(DependencyObject dpo) {
            if (dpo == null || !GetIsEnabled(dpo)) {
                return;
            }
            var angle = GetAngle(dpo);
            if (dpo is Control c) {
                var bg = c.Background == null ? GetDefaultBackgroundBrush(dpo) : c.Background;
                var color1 = MpColorHelpers.GetLighterHexColor(bg.ToHex()).ToWinMediaColor();
                var color2 = MpColorHelpers.GetDarkerHexColor(bg.ToHex()).ToWinMediaColor();
                c.Background = new LinearGradientBrush(color1, color2, angle);
            } else if (dpo is Border b) {
                var bg = b.Background == null ? GetDefaultBackgroundBrush(dpo) : b.Background;
                var color1 = MpColorHelpers.GetLighterHexColor(bg.ToHex()).ToWinMediaColor();
                var color2 = MpColorHelpers.GetDarkerHexColor(bg.ToHex()).ToWinMediaColor();
                b.Background = new LinearGradientBrush(color1, color2, angle);
            }

        }

        private static void Fe_Unloaded(object sender, RoutedEventArgs e) {
            var fe = sender as FrameworkElement;
            fe.Loaded -= Fe_Loaded;
            fe.Unloaded -= Fe_Unloaded;
        }

        private static void Fe_Loaded(object sender, RoutedEventArgs e) {
            var fe = sender as FrameworkElement;

            SetBackground(fe);
        }
    }
}