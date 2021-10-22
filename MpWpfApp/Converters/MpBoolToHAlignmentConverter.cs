using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace MpWpfApp {
    public class MpBoolToHAlignmentConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            if(value == null || parameter == null) {
                return HorizontalAlignment.Left;
            }
            var cl = parameter.ToString().Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);
            if ((bool)value) {
                return GetHorizontalAlignmentFromString(cl[0]);
            }
            return GetHorizontalAlignmentFromString(cl[1]);
        }

        private HorizontalAlignment GetHorizontalAlignmentFromString(string text) {
            if (text.ToLower() == "left") {
                return HorizontalAlignment.Left;
            }
            if (text.ToLower() == "right") {
                return HorizontalAlignment.Right;
            }
            if (text.ToLower() == "center") {
                return HorizontalAlignment.Center;
            }
            if (text.ToLower() == "stretch") {
                return HorizontalAlignment.Stretch;
            }
            return HorizontalAlignment.Left; 
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            return (Visibility)value == Visibility.Visible ? true : false;
        }
    }
}
