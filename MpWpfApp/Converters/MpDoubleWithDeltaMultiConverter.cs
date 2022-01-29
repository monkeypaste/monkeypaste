using System;
using System.Globalization;
using System.Windows.Data;

namespace MpWpfApp {
    public class MpDoubleWithDeltaMultiConverter : IMultiValueConverter {

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture) {
            double v = (double)values[0];
            return v + System.Convert.ToDouble(values[1].ToString());
        }


        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) {
            return null;
        }
    }
}
