using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using MonkeyPaste;

namespace MpWpfApp {
    public class MpStringToIconConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            if (value is string valueStr) {
                if(string.IsNullOrEmpty(valueStr)) {
                    return new Image();
                }
                if(valueStr.IsStringHexColor()) {
                    if(parameter is string paramStr) {
                        var paramParts = paramStr.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);

                    }
                }
                var icon = new Image();
                icon.Source = (BitmapSource)new BitmapImage(new Uri(value.ToString()));
                return icon;
            }
            return new Image();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            return null;
        }
    }
}
