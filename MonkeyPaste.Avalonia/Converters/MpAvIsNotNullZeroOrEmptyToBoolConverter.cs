﻿using Avalonia.Data.Converters;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;

namespace MonkeyPaste.Avalonia {
    public class MpAvIsNotNullZeroOrEmptyToBoolConverter : IValueConverter {
        public static readonly MpAvIsNotNullZeroOrEmptyToBoolConverter Instance = new();

        public object Convert(object value, Type targetType, object? parameter, CultureInfo culture) {
            bool flip = false;
            if(parameter is string paramStr && paramStr.ToLower() == "flip") {
                flip = true;
            }
            if(value is int intVal) {
                return intVal == 0 ? flip : !flip;
            }
            if (value is double doubleVal) {
                return doubleVal == 0 ? flip : !flip;
            }
            if(value is string strVal) {
                return string.IsNullOrEmpty(strVal) ? flip : !flip;
            }
            if (value is ICollection collection) {
                return collection.Count == 0 ? flip : !flip;
            }
            if (value is IEnumerable<object> enumerable) {
                return enumerable.Count() == 0 ? flip : !flip;
            }
            return value == null ? flip : !flip;
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) {
            throw new NotSupportedException();
        }
    }
}