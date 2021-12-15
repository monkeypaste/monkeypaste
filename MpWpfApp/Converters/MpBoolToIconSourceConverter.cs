using System;
using System.Globalization;
using System.Windows.Data;

namespace MpWpfApp {
    public class MpBoolToIconSourceConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            string imgPath = new MpBoolToImageSourceConverter().Convert(value, targetType, parameter, culture).ToString();

            return new MpStringToIconConverter().Convert(value, targetType, parameter, culture);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            return null;
        }
    }
}
