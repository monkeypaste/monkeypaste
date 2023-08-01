using Avalonia.Data.Converters;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Avalonia;
using System;
using System.Globalization;

namespace MonkeyPaste.Avalonia {
    public class MpAvStringBase64ToBitmapConverter : IValueConverter {
        public static readonly MpAvStringBase64ToBitmapConverter Instance = new();

        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture) {
            if (value is string valStr) {
                if (valStr.IsStringBase64()) {
                    double scale = 1.0d;
                    string tint_color = null;
                    if (parameter is string paramStr &&
                        !string.IsNullOrEmpty(paramStr)) {
                        if (paramStr.IsStringHexOrNamedColor()) {
                            tint_color = paramStr;
                        } else if (double.TryParse(paramStr, out scale)) {

                        }
                    }
                    try {
                        var bmp = valStr.ToAvBitmap(scale, tint_color);
                        return bmp;
                    }
                    catch (Exception ex) {
                        MpConsole.WriteTraceLine($"Error converting base64 '{value}' to bmp.", ex);
                    }
                }
            }
            return null;
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) {
            throw new NotSupportedException();
        }
    }

}
