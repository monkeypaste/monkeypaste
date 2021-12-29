using System;
using System.Globalization;
using System.Windows.Data;

namespace MpWpfApp {
    public class MpStringToDateTimeConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            if(value is string valueStr && !string.IsNullOrEmpty(valueStr)) {
                if(DateTime.TryParse(valueStr, out DateTime result)) {
                    return result;
                }                
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            if (value is DateTime dt) {
                return dt.ToString();
            }
            return null;
        }
    }
}
