using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using MonkeyPaste;

namespace MpWpfApp {
    public class MpIsHexColorBrightToBlackOrWhiteBrushConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            if (value == null) {
                return false;
            }
            string valueStr = (string)value;
            return MpColorHelpers.IsBright(valueStr) ? Brushes.Black : Brushes.White;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            return (Visibility)value == Visibility.Visible ? true : false;
        }
    }
}
