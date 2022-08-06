using Avalonia;
using Avalonia.Data.Converters;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Avalonia;
using System;
using System.Globalization;

namespace MonkeyPaste.Avalonia {
    public class MpAvBoolToBrushConverter : IValueConverter {
        public static readonly MpAvBoolToBrushConverter Instance = new();

        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture) {
            if (value == null || parameter == null) {
                return null;
            }

            bool boolVal = (bool)value;
            string[] brushHexParts = parameter.ToString().Split(new string[] { "|" }, StringSplitOptions.RemoveEmptyEntries);

            if (brushHexParts.Length == 0) {
                return null;
            }

            string brushHexOrNamedColorStr = brushHexParts[0];

            if (brushHexParts.Length == 2) {
                brushHexOrNamedColorStr = boolVal ? brushHexParts[0] : brushHexParts[1];
            }

            return MpSystemColors.ConvertFromString(brushHexOrNamedColorStr).ToAvBrush();
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) {
            throw new NotSupportedException();
        }
    }
}
