﻿using Avalonia.Data.Converters;
using MonkeyPaste.Common;
using System;
using System.Globalization;

namespace MonkeyPaste.Avalonia {
    public class MpAvDateTimeToReadableTimeSpanStringConverter : IValueConverter {
        public static readonly MpAvDateTimeToReadableTimeSpanStringConverter Instance = new();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            if (value is DateTime dt) {
                var compare_dt = parameter is DateTime ? (DateTime)parameter : DateTime.Now;
                return (compare_dt - dt).ToReadableTimeSpan();
            }
            return null;
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) {
            throw new NotSupportedException();
        }
    }
}
