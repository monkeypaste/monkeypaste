using Avalonia.Data.Converters;
using MonkeyPaste.Common;
using System;
using System.Globalization;

namespace MonkeyPaste.Avalonia {
    public class MpAvIntCompareToBoolConverter : IValueConverter {
        public static readonly MpAvIntCompareToBoolConverter Instance = new();

        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture) {
            if (value is not int intVal ||
                parameter is not string paramStr ||
                paramStr.Split("|") is not string[] paramParts ||
                paramParts.Length != 2) {
                return false;
            }
            string compOp = paramParts[0];
            int compVal;

            try {
                compVal = System.Convert.ToInt32(paramParts[1]);
            }
            catch (Exception ex) {
                MpDebug.Break($"Error parsing compare val from '{paramParts[1]}'. Falling back to -1. {ex}");
                compVal = -1;
            }
            switch (compOp) {
                case "eq":
                    return intVal == compVal;
                case "gt":
                    return intVal > compVal;
                case "lt":
                    return intVal < compVal;
                default:
                    MpDebug.Break($"Unknown compOp '{compOp}'. Returning false");
                    return false;
            }
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) {
            throw new NotSupportedException();
        }
    }

}
