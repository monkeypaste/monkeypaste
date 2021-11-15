using System;
using System.Globalization;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace MpWpfApp {
    public class MpBase64StringToIconConverter : IValueConverter {
        private static ImageSourceConverter _isc = null;
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            if (value is string valueStr) {
                byte[] byteBuffer = System.Convert.FromBase64String(valueStr);
                if (_isc == null) {
                    _isc = new ImageSourceConverter();
                }
                var bmpSrc = (BitmapSource)_isc.ConvertFrom(byteBuffer);
                bmpSrc.Freeze();
                var img = new Image {
                    Source = bmpSrc,
                    Stretch = Stretch.Fill
                };
                
                return img;
            }
            return new Image();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            return null;
        }
    }
}
