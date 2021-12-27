using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace MpWpfApp {
    public class MpBase64ToImageBrushConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            if (value is string valueStr) {
                var bmpSrc = valueStr.ToBitmapSource();
                bmpSrc.Freeze();
                return new ImageBrush() {
                    ImageSource = bmpSrc,
                    Stretch = Stretch.Fill,
                    
                };
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            return null;
        }
    }
}
