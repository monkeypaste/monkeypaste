using Avalonia.Data.Converters;
using MonkeyPaste.Common;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;

namespace MonkeyPaste.Avalonia {
    public class MpAvMultiSliderValueToLengthConverter : IMultiValueConverter {
        public static readonly MpAvMultiSliderValueToLengthConverter Instance = new();

        public object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture) {
            if (values.Count == 4 &&
                values.All(x => x is double) &&
                values.Cast<double>().ToArray() is double[] dblVals) {
                double min = dblVals[0];
                double max = dblVals[1];
                double cur = dblVals[2];
                double max_length = dblVals[3];

                double percent = cur / (max - min);
                double length = max_length * percent;
                return length;
            }
            return 0;
        }
    }
}
