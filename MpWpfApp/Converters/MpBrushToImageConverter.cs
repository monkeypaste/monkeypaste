using MonkeyPaste;
using System;
using System.Globalization;
using System.IO;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using MonkeyPaste.Common.Wpf;

namespace MpWpfApp {
    public class MpBrushToImageConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            if (value is Brush brush) {
                var bmpSrc = (BitmapSource)new BitmapImage(
                    new Uri(MpPrefViewModel.Instance.AbsoluteResourcesPath + @"/Images/texture.png"));
               return bmpSrc.Tint(brush);
            }
            return new Image();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            return null;
        }
    }
}
