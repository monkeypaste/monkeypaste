using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Xamarin.Forms;

namespace MonkeyPaste {
    public class MpStringToColorConverter : IValueConverter {
        public object Convert(object value, Type targetType,
                              object parameter, CultureInfo culture) {
            return Color.FromHex(value.ToString());
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            return ((Color)value).ToHex();
        }
    }
}
    
