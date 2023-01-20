using Avalonia;
using Avalonia.Data.Converters;
using System;
using System.Globalization;
using MonkeyPaste.Common;

namespace MonkeyPaste.Avalonia {
    public class MpAvDateTimeToReadableTimeSpanStringConverter : IValueConverter {
        public static readonly MpAvDateTimeToReadableTimeSpanStringConverter Instance = new();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            if(value is DateTime dt) {
                var compare_dt = parameter is DateTime ? (DateTime)parameter : DateTime.Now;
                return (compare_dt - dt).ToReadableTimeSpan();
            }
            return null;
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) {
            throw new NotSupportedException();
        }
    }
    
    public class MpAvDateTimeToStringConverter : IValueConverter {
        public static readonly MpAvDateTimeToStringConverter Instance = new();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            if(value is DateTime dt) {
                string format = parameter is string ? parameter as string : "MM/dd/yyyy hh:mm tt";
                return dt.ToString(format);
            }
            return null;
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) {
            throw new NotSupportedException();
        }
    }
}
