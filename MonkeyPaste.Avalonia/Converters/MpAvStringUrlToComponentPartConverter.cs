using Avalonia.Data.Converters;
using MonkeyPaste.Common;
using System;
using System.Globalization;

namespace MonkeyPaste.Avalonia {
    public class MpAvStringUrlToComponentPartConverter : IValueConverter {
        public static readonly MpAvStringUrlToComponentPartConverter Instance = new();

        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture) {
            if (value is string valStr && Uri.IsWellFormedUriString(valStr, UriKind.Absolute)) {
                value = new Uri(valStr, UriKind.Absolute);
            }
            if (value is not Uri valUri) {
                return value;
            }
            string compType = string.IsNullOrEmpty(parameter.ToStringOrEmpty()) ? "query" : parameter.ToString();
            switch (compType) {
                case "query":
                    return valUri.Query;
            }
            return value;
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) {
            throw new NotSupportedException();
        }
    }

}
