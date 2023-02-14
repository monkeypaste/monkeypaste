using Avalonia.Data.Converters;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace MonkeyPaste.Avalonia {
    public class MpAvCollectionIsNotEmptyToBoolConverter : IValueConverter {
        public static readonly MpAvCollectionIsNotEmptyToBoolConverter Instance = new();

        public object Convert(object value, Type targetType, object? parameter, CultureInfo culture) {
            bool isNotEmpty = false;

            if (value is ICollection collection) {
                isNotEmpty = collection.Count > 0;
            } else if (value is IEnumerable<object> enumerable) {
                isNotEmpty = enumerable.Count() > 0;
            }
            if (parameter is string paramStr &&
                paramStr.ToLower() == "flip") {
                return !isNotEmpty;
            }
            return isNotEmpty;
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) {
            throw new NotSupportedException();
        }
    }
}
