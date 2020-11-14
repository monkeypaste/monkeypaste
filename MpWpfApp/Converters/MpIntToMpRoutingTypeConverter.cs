using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace MpWpfApp {
    public class MpIntToMpRoutingTypeConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            return (int)value; 
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            return (MpRoutingType)value;
        }
    }
}
