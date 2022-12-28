using Avalonia;
using Avalonia.Data.Converters;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using MonkeyPaste.Common;

namespace MonkeyPaste.Avalonia {
    public class MpAvMillisecondsToTimeSpanStringConverter : IValueConverter {
        public static readonly MpAvMillisecondsToTimeSpanStringConverter Instance = new();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            if(value is int valMs) {
                return TimeSpan.FromMilliseconds(valMs).ToString();
            }
            return string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }
    }
}
