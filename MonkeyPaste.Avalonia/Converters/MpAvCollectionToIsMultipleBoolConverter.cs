using Avalonia.Controls;
using Avalonia.Data.Converters;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace MonkeyPaste.Avalonia {
    public class MpAvCollectionToIsMultipleBoolConverter : IValueConverter {
        public static readonly MpAvCollectionToIsMultipleBoolConverter Instance = new();

        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture) {
            bool flip = false;
            if (parameter is string paramStr && paramStr.ToLowerInvariant() == "flip") {
                flip = true;
            }
            int count = 0;
            if (value is ICollection collection) {
                count = collection.Count;
            }
            if (value is IEnumerable<object> enumerable) {
                count = enumerable.Count();
            }
            return count > 1 ? !flip : flip;
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }
    }
}
