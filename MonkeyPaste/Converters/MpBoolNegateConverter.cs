﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Xamarin.Forms;

namespace MonkeyPaste {
    public class MpBoolNegateConverter : IValueConverter {
        public object Convert(object value, Type targetType,
                              object parameter, CultureInfo culture) {
            if(value == null) {
                return true;
            }
            return !((bool)value);
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            if (value == null) {
                return false;
            }
            return !((bool)value);
        }
    }
}
    
