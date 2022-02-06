using System;
using System.Globalization;
using System.Windows.Data;

namespace MpWpfApp {
    public class MpObjectToTooltipConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            if(value is string valueStr) {
                return valueStr;
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            return null;
        }
    }
}
