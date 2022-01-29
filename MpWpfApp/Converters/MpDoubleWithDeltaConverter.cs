using System;
using System.Globalization;
using System.Windows.Data;

namespace MpWpfApp {
    public class MpDoubleWithDeltaConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            double v = (double)value;
            return v + System.Convert.ToDouble(parameter.ToString());
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            return 0;
        }
    }
}
