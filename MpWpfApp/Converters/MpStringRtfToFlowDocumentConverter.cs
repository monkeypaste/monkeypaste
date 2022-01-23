using System;
using System.Globalization;
using System.Windows.Data;

namespace MpWpfApp {
    public class MpStringRtfToFlowDocumentConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            return value.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            return System.Convert.ToInt32(value.ToString());
        }
    }
}
