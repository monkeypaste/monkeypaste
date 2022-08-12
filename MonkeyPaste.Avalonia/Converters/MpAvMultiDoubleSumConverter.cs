using Avalonia;
using Avalonia.Data.Converters;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using MonkeyPaste.Common;
using System.Diagnostics;

namespace MonkeyPaste.Avalonia {
    public class MpAvMultiDoubleSumConverter : IMultiValueConverter {
        public static readonly MpAvMultiDoubleSumConverter Instance = new();

        public object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture) {
            if(values != null) {
                double sum = values.Cast<double>().Where(x => x.IsNumber()).Sum();
                return sum;
            }
            return 0;
        }
    }
}
