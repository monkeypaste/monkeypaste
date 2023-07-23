using Avalonia.Data.Converters;
using System;
using System.Globalization;

namespace MonkeyPaste.Avalonia {
    public class MpAvDoubleToBoolConverter : IValueConverter {
        public static readonly MpAvDoubleToBoolConverter Instance = new();

        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture) {
            if(value is not double valDbl ||
                parameter is not string paramStr) {
                return false;
            }
            return valDbl == System.Convert.ToDouble(paramStr);
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }
    }
}
