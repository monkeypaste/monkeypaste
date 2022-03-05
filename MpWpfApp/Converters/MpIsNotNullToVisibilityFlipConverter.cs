using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace MpWpfApp {
    public class MpIsNotNullToVisibilityFlipConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            Visibility notVisibleType = parameter == null ? Visibility.Collapsed : parameter.ToString().ToLower() == "hide" ? Visibility.Hidden : Visibility.Collapsed;
            return value == null ? Visibility.Visible : notVisibleType;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            return null;
        }
    }
}
