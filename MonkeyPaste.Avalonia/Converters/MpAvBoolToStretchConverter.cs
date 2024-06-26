using Avalonia.Data.Converters;
using MonkeyPaste.Common;
using System;
using System.Globalization;
using System.Windows.Media;

namespace MonkeyPaste.Avalonia {
    public class MpAvBoolToStretchConverter : IValueConverter {
        public static readonly MpAvBoolToStretchConverter Instance = new();

        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture) {
            if (value is not bool boolVal ||
                parameter is not string paramStr ||
                paramStr.SplitNoEmpty("|") is not { } paramParts) {
                return Stretch.Uniform;
            }

            var true_val = paramParts[0].ToEnum<Stretch>();
            var false_val = paramParts[1].ToEnum<Stretch>();
            return boolVal ? true_val : false_val;
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) {
            throw new NotSupportedException();
        }
    }   
}
