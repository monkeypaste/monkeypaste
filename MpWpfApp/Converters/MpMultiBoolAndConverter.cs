﻿using System;
using System.Globalization;
using System.Linq;
using System.Windows.Data;

namespace MpWpfApp {
    public class MpMultiBoolAndConverter : IMultiValueConverter {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture) {
            if(values == null || values.Length == 0) {
                return false;
            }
            return values.All(x => (bool)x);
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }
    }
}