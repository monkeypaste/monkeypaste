using System;
using System.Globalization;
using System.IO;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using MonkeyPaste.Common.Wpf;

namespace MpWpfApp {
    public class MpBase64StringToBitmapSourceConverter : IValueConverter {
        //private static ImageSourceConverter _isc = null;
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            if (value is string valueStr) {
                //byte[] byteBuffer = System.Convert.FromBase64String(valueStr);
                //if(_isc == null) {
                //    _isc = new ImageSourceConverter();
                //}
                //var bmpSrc = (BitmapSource)_isc.ConvertFrom(byteBuffer);
                //bmpSrc.Freeze();
                //return bmpSrc;     
                return valueStr.ToBitmapSource();
            }
            return new Image();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            return null;
        }
    }
}
