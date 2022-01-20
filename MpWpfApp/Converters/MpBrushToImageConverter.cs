using System;
using System.Globalization;
using System.IO;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace MpWpfApp {
    public class MpBrushToImageConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            if (value is Brush brush) {
                var bmpSrc = (BitmapSource)new BitmapImage(
                    new Uri(Properties.Settings.Default.AbsoluteResourcesPath + @"/Images/texture.png"));
               return MpHelpers.TintBitmapSource(bmpSrc, ((SolidColorBrush)brush).Color);
            }
            return new Image();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            return null;
        }
    }
}
