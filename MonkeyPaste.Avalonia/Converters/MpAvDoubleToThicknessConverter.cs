using Avalonia;
using Avalonia.Data.Converters;
using System;
using System.Globalization;

namespace MonkeyPaste.Avalonia {
    public class MpAvDoubleToThicknessConverter : IValueConverter {
        public static readonly MpAvDoubleToThicknessConverter Instance = new();

        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture) {
            if (value is double valDbl) {
                return new Thickness(valDbl, valDbl, valDbl, valDbl);
            }
            return new Thickness();
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) {
            throw new NotSupportedException();
        }
    }
}
