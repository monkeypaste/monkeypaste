using System;
using System.Globalization;
using System.Windows.Data;

namespace MpWpfApp {
    public class MpEnumStringToKeyLiteralStringConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            if (value is string valueStr) {
                if(string.IsNullOrEmpty(valueStr)) {
                    return string.Empty;
                }
                return MpHelpers.Instance.GetKeyLiteral(
                        MpHelpers.Instance.ConvertStringToKey(valueStr));
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            return null;
        }
    }
}
