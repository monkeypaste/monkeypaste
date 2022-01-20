using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Xamarin.Forms;

namespace MonkeyPaste {
    public class MpTextColorFromBackgroundColorConverterFromString : IValueConverter {
        public object Convert(object value, Type targetType,
                              object parameter, CultureInfo culture) {
            if(value == null || string.IsNullOrEmpty(value.ToString())) {
                return Color.Black;
            }
            return MpHelpers.IsBright(Color.FromHex(value.ToString())) ? Color.Black : Color.White;
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            return ((Color)value).ToHex();
        }
    }
}
    
