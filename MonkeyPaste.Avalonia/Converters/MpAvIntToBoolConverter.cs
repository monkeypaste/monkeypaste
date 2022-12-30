using Avalonia;
using Avalonia.Data.Converters;
using System;
using System.Globalization;

namespace MonkeyPaste.Avalonia {
    public class MpAvIntToBoolConverter : IValueConverter {
        public static readonly MpAvIntToBoolConverter Instance = new();

        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture) {
            int paramVal = parameter == null ? -1:int.Parse(parameter.ToString());
            if(value is int intVal) {
                return intVal == paramVal;
            }
            return false;
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) {
            int paramVal = parameter == null ? -1 : int.Parse(parameter.ToString());
            if (value is bool boolVal) {
                if(boolVal) {
                    return paramVal;
                }
            }
            return -1;
        }
    }
}
