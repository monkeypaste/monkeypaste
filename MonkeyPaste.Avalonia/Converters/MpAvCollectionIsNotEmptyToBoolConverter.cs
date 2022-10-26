using Avalonia;
using Avalonia.Data.Converters;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Avalonia;
using System;
using System.Collections;
using System.Globalization;

namespace MonkeyPaste.Avalonia {
    public class MpAvCollectionIsNotEmptyToBoolConverter : IValueConverter {
        public static readonly MpAvCollectionIsNotEmptyToBoolConverter Instance = new();

        public object Convert(object value, Type targetType, object? parameter, CultureInfo culture) {
            if(value is ICollection collection) {
                return collection.Count > 0;
            }
            return false;
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) {
            throw new NotSupportedException();
        }
    }
}
