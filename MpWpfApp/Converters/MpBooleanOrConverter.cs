﻿using System;
using System.Windows.Data;

namespace MpWpfApp {
    public class MpBooleanOrConverter : IMultiValueConverter {
        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
            foreach (object value in values) {
                if ((value is bool) && (bool)value == true) {
                    return true;
                }
            }
            return false;
        }
        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture) {
            throw new NotSupportedException("BooleanAndConverter is a OneWay converter.");
        }
    }
}
