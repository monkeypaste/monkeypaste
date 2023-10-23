using Avalonia.Data.Converters;
using System;
using System.Globalization;

namespace MonkeyPaste.Avalonia {
    public class MpAvEnumToBoolConverter : IValueConverter {
        public static readonly MpAvEnumToBoolConverter Instance = new();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            if (value == null ||
                value.ToString() is not string valStr ||
                parameter is not string paramStr) {
                return false;
            }
            return valStr == paramStr;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }
    }
}
