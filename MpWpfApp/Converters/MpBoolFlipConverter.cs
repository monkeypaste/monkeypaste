﻿using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace MpWpfApp {
    public class MpBoolFlipConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            if (value == null) {
                return false;
            }
            return !(bool)value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            return (Visibility)value == Visibility.Visible ? true : false;
        }
    }
}