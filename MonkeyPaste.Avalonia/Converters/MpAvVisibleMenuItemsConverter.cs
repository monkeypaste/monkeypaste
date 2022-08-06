using Avalonia;
using Avalonia.Data.Converters;
using System;
using System.Collections;
using System.Globalization;
using MonkeyPaste;
using System.Linq;
using System.Collections.Generic;

namespace MonkeyPaste.Avalonia {
    public class MpAvVisibleMenuItemsConverter : IValueConverter {
        public static readonly MpAvVisibleMenuItemsConverter Instance = new();

        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture) {
            if(value is IList<MpMenuItemViewModel> sil) {
                return sil.Where(x => x.IsVisible);
            }
            return null;
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) {
            throw new NotSupportedException();
        }
    }
}
