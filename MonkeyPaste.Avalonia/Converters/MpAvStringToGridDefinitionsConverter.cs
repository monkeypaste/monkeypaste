using Avalonia.Controls;
using Avalonia.Data.Converters;
using System;
using System.Globalization;

namespace MonkeyPaste.Avalonia {
    public class MpAvStringToGridDefinitionsConverter : IValueConverter {
        public static readonly MpAvStringToGridDefinitionsConverter Instance = new();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            bool isRow = true;
            string valueStr = string.Empty;
            if (value is string) {
                valueStr = value.ToString();
                if (parameter is string paramStr) {
                    isRow = paramStr.ToLowerInvariant() == "row";
                }
            }
            return isRow ?
                new RowDefinitions(valueStr) :
                new ColumnDefinitions(valueStr);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            if (value is DateTime dt) {
                return dt.ToString();
            }
            if (value is DateTimeOffset dto) {
                return dto.DateTime.ToString();
            }
            return null;
        }
    }
}
