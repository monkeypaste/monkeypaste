using Avalonia.Data.Converters;
using MonkeyPaste.Common;
using System;
using System.Globalization;
using System.Linq;

namespace MonkeyPaste.Avalonia {
    public class MpAvBoolToOpacityConverter : IValueConverter {
        public static readonly MpAvBoolToOpacityConverter Instance = new();

        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture) {
            if (value is bool boolVal) {
                double falseOpacity = 0;
                double trueOpacity = 1;
                if (parameter is string paramStr &&
                    paramStr.SplitNoEmpty("|") is string[] paramParts &&
                    paramParts.Select(x => x.ParseOrConvertToDouble(0)).ToArray() is double[] opacityParts) {
                    falseOpacity = opacityParts[0];
                    trueOpacity = opacityParts[1];
                }
                return boolVal ? trueOpacity : falseOpacity;
            }
            return 1;
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) {
            throw new NotSupportedException();
        }
    }
}
