using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Data.Converters;
using Avalonia.Input;
using MonkeyPaste.Common;
using System;
using System.Globalization;
using System.Linq;

namespace MonkeyPaste.Avalonia {
    public class MpAvKeyStringToKeyGestureConverter : IValueConverter {
        public static readonly MpAvKeyStringToKeyGestureConverter Instance = new();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            if (value is not string valStr) {
                return null;
            }
            var result = Mp.Services.KeyConverter.ConvertStringToKeyLiteralSequence(valStr);
            if (result != null && result.Any()) {

                return KeyGesture.Parse(string.Join("+", result.First()));
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }
    }
}
