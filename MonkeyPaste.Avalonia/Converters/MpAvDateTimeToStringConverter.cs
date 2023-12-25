using Avalonia.Data.Converters;
using MonkeyPaste.Common;
using System;
using System.Globalization;

namespace MonkeyPaste.Avalonia {
    public class MpAvDateTimeToStringConverter : IValueConverter {
        public static readonly MpAvDateTimeToStringConverter Instance = new();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            if (value is DateTime dt) {
                if (parameter.ToStringOrEmpty() == "dateandtime") {
                    parameter = UiStrings.CommonDateTimeFormat;
                }
                string format = parameter is string ? parameter as string : UiStrings.CommonDateFormat;
                return dt.ToString(format, UiStrings.Culture);
            }
            return null;
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) {
            throw new NotSupportedException();
        }
    }
}
