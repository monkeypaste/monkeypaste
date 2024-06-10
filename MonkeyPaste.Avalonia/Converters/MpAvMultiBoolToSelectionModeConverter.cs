using Avalonia.Controls;
using Avalonia.Data.Converters;
using MonkeyPaste.Common;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace MonkeyPaste.Avalonia {
    public class MpAvMultiBoolToSelectionModeConverter : IMultiValueConverter {
        public static readonly MpAvMultiBoolToSelectionModeConverter Instance = new();
        public object Convert(IList<object> values, Type targetType, object parameter, CultureInfo culture) {
            if(values is null ||
                values.OfType<bool>() is not { } boolVals ||
                parameter is not string paramStr ||
                paramStr.SplitNoEmpty("|") is not { } paramParts) {
                return SelectionMode.Single;
            }
            bool and_op = paramParts[0].ToLower() == "and";
            var true_mode = paramParts[1].ToEnum<SelectionMode>();
            var false_mode = paramParts[2].ToEnum<SelectionMode>();
            bool is_true =
                and_op ? boolVals.All(x => x) : boolVals.Any(x => x);
            return is_true ? true_mode : false_mode;
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) {
            throw new NotSupportedException();
        }

        
    }
}
