﻿using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace MpWpfApp {
    public class MpBrushToColorConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            if (value == null || value.GetType() != typeof(Brush) || !value.GetType().IsSubclassOf(typeof(Brush))) {
                return Brushes.Transparent;
            }
            var br = value as Brush;
            if(parameter == null || parameter.GetType() != typeof(double)) {
                return (br as SolidColorBrush).Color;
            }

            var c = (br as SolidColorBrush).Color;

            //between 0-1
            double opacity = (double)parameter;
            byte alpha = 0;
            if(opacity > 0) {
                alpha = (byte)(255 / (byte)opacity);
            }

            return Color.FromArgb(alpha, c.R, c.G, c.B);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            return (Visibility)value == Visibility.Visible ? true : false;
        }
    }
}