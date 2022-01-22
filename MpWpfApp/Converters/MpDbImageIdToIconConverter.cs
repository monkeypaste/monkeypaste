using System;
using System.Globalization;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace MpWpfApp {
    public class MpDbImageIdToIconConverter : IValueConverter {
        //returns primary source by default but secondary w/ parameter of 'SecondarySource' 
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            return new Image() {
                Source = new MpDbImageIdToBitmapSourceConverter().Convert(value,targetType,parameter,culture) as BitmapSource
            };
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            return null;
        }
    }
}
