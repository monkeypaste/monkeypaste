using System;
using System.Globalization;
using System.Windows.Data;

namespace MpWpfApp {
    public class MpDateTimeFormatToStringConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            DateTime dt = DateTime.Now;
            if (value is string valueStr) {
            
                if(parameter is string paramStr) {
                    if(DateTime.TryParse(paramStr, out dt)) {

                    }
                }
                return dt.ToString(valueStr);
            }
            return dt.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            return !(bool)value;
        }
    }
}
