﻿using System;
using System.Globalization;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace MpWpfApp {
    public class MpStringToIconConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            if (value is string) {
                var icon = new Image();
                icon.Source = (BitmapSource)new BitmapImage(new Uri(Properties.Settings.Default.AbsoluteResourcesPath + value.ToString()));
                return icon;
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            return null;
        }
    }
}