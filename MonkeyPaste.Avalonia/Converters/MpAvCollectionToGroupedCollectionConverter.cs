using Avalonia.Collections;
using Avalonia.Data.Converters;
using System;
using System.Collections;
using System.Globalization;

namespace MonkeyPaste.Avalonia {
    public class MpAvCollectionToGroupedCollectionConverter : IValueConverter {
        public static readonly MpAvCollectionToGroupedCollectionConverter Instance = new();

        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture) {
            if (value is IEnumerable collection &&
                parameter is string memberPath) {
                var dgcv = new DataGridCollectionView(collection);
                dgcv.GroupDescriptions.Add(new DataGridPathGroupDescription(memberPath));
                return dgcv;
            }
            return null;
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) {
            throw new NotSupportedException();
        }
    }
}
