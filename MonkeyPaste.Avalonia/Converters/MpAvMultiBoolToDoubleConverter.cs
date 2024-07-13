using Avalonia.Data.Converters;
using MonkeyPaste.Common;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace MonkeyPaste.Avalonia {
    public class MpAvMultiBoolToDoubleConverter : IMultiValueConverter {
        public static readonly MpAvMultiBoolToDoubleConverter Instance = new();

        public object Convert(IList<object> values, Type targetType, object parameter, CultureInfo culture) {
            if (parameter is not string paramStr ||
                paramStr.SplitNoEmpty("|") is not string[] paramParts ||
                paramParts.Length != 3) {
                return 0;
            }
            string op = paramParts[0].ToLowerInvariant();
            double true_val = System.Convert.ToDouble(paramParts[1]);
            double false_val = System.Convert.ToDouble(paramParts[2]);

            bool is_true =
                    op == "and" ?
                        (bool)BoolConverters.And.Convert(values, targetType, null, culture) :
                        (bool)BoolConverters.Or.Convert(values, targetType, null, culture);

            return is_true ? true_val : false_val;
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) {
            throw new NotSupportedException();
        }

    }
}
