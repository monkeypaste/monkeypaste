using Avalonia.Data.Converters;
using System;
using System.Collections;
using System.Globalization;

namespace MonkeyPaste.Avalonia {
    public class MpAvCollectionCountToStringConverter : IValueConverter {
        public static readonly MpAvCollectionIsNotEmptyToBoolConverter Instance = new();

        public object Convert(object value, Type targetType, object? parameter, CultureInfo culture) {
            if (value is ICollection collection) {
                return collection.Count.ToString();
            }
            return null;
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) {
            throw new NotSupportedException();
        }
    }
}
