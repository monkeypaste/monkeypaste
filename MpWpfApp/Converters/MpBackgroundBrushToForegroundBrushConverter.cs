using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace MpWpfApp {
    public class MpBackgroundBrushToForegroundBrushConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            if (value == null) {
                return Brushes.Red;
            }
            return MpHelpers.IsBright((value as SolidColorBrush).Color) ? Brushes.Black : Brushes.White;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            return value;
        }
    }
}
