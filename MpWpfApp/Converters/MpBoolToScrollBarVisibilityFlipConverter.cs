using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace MpWpfApp {
    public class MpBoolToScrollBarVisibilityFlipConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            if (value == null) {
                return ScrollBarVisibility.Hidden;
            }
            var visibleOrAuto = parameter != null && parameter.ToString() == "Auto" ? ScrollBarVisibility.Auto : ScrollBarVisibility.Visible;
            return (bool)value ? ScrollBarVisibility.Hidden : visibleOrAuto;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            return (Visibility)value == Visibility.Visible ? true : false;
        }
    }
}
