using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media.Imaging;
using Windows.Foundation.Collections;
using MonkeyPaste;

namespace MpWpfApp {

    public class MpStringResourceToImageSourceConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            if (value is string valueStr) {
                if(valueStr.IsStringResourcePath()) {
                    return valueStr;
                }
                return @"/Resources" + value.ToString();
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            return null;
        }
    }
}
