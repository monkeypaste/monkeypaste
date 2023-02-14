using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
//using Xamarin.Forms;

namespace MonkeyPaste {
    public class MpTextColorFromBackgroundColorConverter : IValueConverter {
        public object Convert(object value, Type targetType,
                              object parameter, CultureInfo culture) {
            if(value == null || value.GetType() != typeof(Color)) {
                return Color.Black;
            }
            return MpHelpers.IsBright((Color)value) ? Color.Black : Color.White;
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            return ((Color)value).ToHex();
        }
    }
}
    
