using Avalonia.Data.Converters;
using System;
using System.Globalization;
using System.Linq;

namespace MonkeyPaste.Avalonia {
    public class MpAvItemToItemsSourceConverter : IValueConverter {
        public static readonly MpAvItemToItemsSourceConverter Instance = new();

        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture) {
            if (value == null)
                return null;

            return new object[] { value }.ToList();
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) {
            throw new NotSupportedException();
        }
    }
}
