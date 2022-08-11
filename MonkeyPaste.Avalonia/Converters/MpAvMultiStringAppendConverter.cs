using Avalonia;
using Avalonia.Data.Converters;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;

namespace MonkeyPaste.Avalonia {
    public class MpAvMultiStringAppendConverter : IMultiValueConverter {
        public static readonly MpAvMultiStringAppendConverter Instance = new();

        public object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture) {
            if(values is List<string> strList) {
                return string.Join(string.Empty, strList);
            }
            return string.Empty;
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) {
            throw new NotSupportedException();
        }
    }
}
