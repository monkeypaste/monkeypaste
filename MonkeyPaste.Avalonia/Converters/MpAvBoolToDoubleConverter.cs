using Avalonia.Data.Converters;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Plugin;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace MonkeyPaste.Avalonia {
    public class MpAvBoolToDoubleConverter : IValueConverter {
        public static readonly MpAvBoolToDoubleConverter Instance = new();

        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture) {
            if (value is bool boolVal) {
                double true_val = 1;
                double false_val = 0;
                if (parameter is string paramStr &&
                    paramStr.SplitNoEmpty("|") is string[] paramParts &&
                    paramParts.Select(x => x.ParseOrConvertToDouble(0)).ToArray() is double[] val_parts) {
                    true_val = val_parts[0];
                    false_val = val_parts[1];
                }
                return boolVal ? true_val : false_val;
            }
            return 1;
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) {
            throw new NotSupportedException();
        }
    }

    public class MpAvMultiBoolToDoubleConverter : IMultiValueConverter {
        public static readonly MpAvMultiBoolToDoubleConverter Instance = new();

        public object Convert(IList<object> values, Type targetType, object parameter, CultureInfo culture) {
            if (parameter is not string paramStr ||
                paramStr.SplitNoEmpty("|") is not string[] paramParts ||
                paramParts.Length != 3) {
                return 0;
            }
            string op = paramParts[0].ToLower();
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
