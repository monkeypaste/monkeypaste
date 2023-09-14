using Avalonia.Data.Converters;
using Avalonia.Input;
using MonkeyPaste.Common.Avalonia;
using System;
using System.Globalization;
using System.Linq;

namespace MonkeyPaste.Avalonia {
    public class MpAvStringKeyLiteralToIsModBoolConverterConverter : IValueConverter {
        public static readonly MpAvStringKeyLiteralToIsModBoolConverterConverter Instance = new();
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            if (value is not string literalStr) {
                return false;
            }
            var kl = Mp.Services.KeyConverter.ConvertStringToKeySequence<Key>(literalStr);
            if (kl.Any() &&
                kl.First().FirstOrDefault() is Key key) {
                return key.IsModKey();
            }
            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            return null;
        }
    }
}
