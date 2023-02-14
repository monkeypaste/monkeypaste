using Avalonia.Data.Converters;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Avalonia;
using System;
using System.Globalization;

namespace MonkeyPaste.Avalonia {
    public class MpAvStringBase64ToBitmapConverter : IValueConverter {
        public static readonly MpAvStringBase64ToBitmapConverter Instance = new();

        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture) {
            double scale = 1.0d;
            if (value is string valStr) {
                if (parameter is string paramStr && double.TryParse(paramStr, out scale)) {

                }
                if (valStr.IsStringBase64()) {
                    return valStr.ToAvBitmap(scale);
                }
            }
            return null;
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) {
            throw new NotSupportedException();
        }
    }

}
