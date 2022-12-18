using Avalonia;
using Avalonia.Collections;
using Avalonia.Data.Converters;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.ComponentModel;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Threading;

namespace MonkeyPaste.Avalonia {
    public class MpAvCollectionToGroupedCollectionConverter : IValueConverter {
        public static readonly MpAvCollectionToGroupedCollectionConverter Instance = new();

        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture) {
            if(value is IEnumerable collection &&
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
