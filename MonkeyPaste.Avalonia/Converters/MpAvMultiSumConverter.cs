using Avalonia;
using Avalonia.Data.Converters;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;

namespace MonkeyPaste.Avalonia {
    public class MpAvMultiSumConverter : IMultiValueConverter {
        public static readonly MpAvMultiSumConverter Instance = new();

        public object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture) {
            if(values is List<double> doubleVals) {
                if(doubleVals.Any(x=>x > 0)) {
                    Debugger.Break();
                }
                return doubleVals.Sum();
            }
            return 0;
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) {
            throw new NotSupportedException();
        }
    }
}
