using System;
using System.Globalization;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace MpWpfApp {
    public class MpStringHexToIconConverter : IValueConverter {
        //returns primary source by default but secondary w/ parameter of 'SecondarySource' 
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            var bmpSrc = new MpStringHexToImageSourceConverter().Convert(value, targetType, parameter, culture);
            if(bmpSrc == null) {
                return null;
            }
            return new Image() {
                Source = bmpSrc as BitmapSource
            };
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            return null;
        }
    }
}
