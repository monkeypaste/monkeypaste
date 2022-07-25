using Avalonia;
using Avalonia.Data.Converters;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace MonkeyPaste.Avalonia {
    public class MpAvMultiBoolConverter : IMultiValueConverter {
        public static readonly MpAvMultiBoolConverter Instance = new();

        public object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture) {
            string op = "AND";
            if (parameter != null && parameter.ToString() == "Or") {
                op = "Or";
            }

            bool isTrue = op == "AND";
            foreach (object value in values) {
                if (value is bool) {
                    switch (op) {
                        case "AND":
                            isTrue = isTrue && (bool)value;
                            break;
                        case "Or":
                            isTrue = isTrue || (bool)value;
                            break;
                    }
                }
            }

            //if (parameter != null) {
            //    return visible ? Visibility.Visible : Visibility.Hidden;
            //}
            return isTrue;
        }
    }
}
