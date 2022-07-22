using System;
using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Avalonia;

namespace MonkeyPaste.Avalonia {
    public class MpAvStringBase64ToBitmapConverter : IValueConverter {
        public static readonly MpAvStringBase64ToBitmapConverter Instance = new();

        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture) {
            if(value is string valStr) {
                if(valStr.IsStringBase64()) {
                    return valStr.ToAvBitmap();
                }
            }
            return null;
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) {
            throw new NotSupportedException();
        }
    }

}
