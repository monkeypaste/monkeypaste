using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace MpWpfApp {
    public class MpStringToBrushConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            return (Brush)new BrushConverter().ConvertFrom(value.ToString());
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            return value.ToString();
        }
    }
}
