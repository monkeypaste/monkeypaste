using Avalonia.Data.Converters;
using System;
using System.Globalization;

namespace MonkeyPaste.Avalonia {
    public class MpAvStringToWindowTitleConverter : IValueConverter {
        public static readonly MpAvStringToWindowTitleConverter Instance = new();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            string name = string.Empty;
            if (value is string valueStr) {
                name = valueStr;
            }
            return name.ToWindowTitleText();
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
