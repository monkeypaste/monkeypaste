using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace MpWpfApp {
    public class MpBoolToScrollBarVisibilityConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            if(value == null) {
                return ScrollBarVisibility.Hidden;
            }
            return (bool)value ? ScrollBarVisibility.Auto : ScrollBarVisibility.Hidden;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            return (Visibility)value == Visibility.Visible ? true : false;
        }
    }
}
