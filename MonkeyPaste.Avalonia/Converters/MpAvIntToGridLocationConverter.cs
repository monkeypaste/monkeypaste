using Avalonia.Data.Converters;
using MonkeyPaste.Common;
using System;
using System.Globalization;

namespace MonkeyPaste.Avalonia {
    public class MpAvIntToGridLocationConverter : IValueConverter {
        public static readonly MpAvIntToGridLocationConverter Instance = new();

        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture) {
            if (value is not int itemIdx ||
                parameter is not string paramStr ||
                paramStr.SplitNoEmpty("|") is not string[] paramParts) {
                return 0;
            }
            bool is_row = paramParts[0].ToLower() == "row";
            int col_count = System.Convert.ToInt32(paramParts[1]);
            if (is_row) {
                return (int)Math.Floor((double)itemIdx / (double)col_count);
            }
            return itemIdx % col_count;
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) {
            throw new NotSupportedException();
        }
    }
}
