using Avalonia.Data.Converters;
using System;
using System.Collections;
using System.Globalization;
using System.Linq;

namespace MonkeyPaste.Avalonia {
    public class MpAvCollectionToVisibleCollectionConverter : IValueConverter {
        public static readonly MpAvCollectionToVisibleCollectionConverter Instance = new();

        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture) {
            if (value is not IEnumerable collection) {
                return default;
            }
            return collection.OfType<MpAvIIsVisibleViewModel>().Where(x => x.IsVisible);
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) {
            throw new NotSupportedException();
        }
    }
}
