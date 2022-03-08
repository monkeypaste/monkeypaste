using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using MonkeyPaste;

namespace MpWpfApp {
    public class MpDoesFileOrDirectoryExistToVisibilityConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            if (value == null) {
                return false;
            }
            return value.ToString().IsFileOrDirectory() ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            return (Visibility)value == Visibility.Visible ? true : false;
        }
    }
}
