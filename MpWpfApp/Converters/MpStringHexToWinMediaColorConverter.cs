using MonkeyPaste;
using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace MpWpfApp {
    public class MpStringHexToWinMediaColorConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            if (value.GetType() != typeof(string) || !value.ToString().IsStringHexColor()) {
                return Colors.Transparent;
            }
            return value.ToString().ToWinMediaColor();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            return ((Color)value).ToHex();
        }
    }
}
