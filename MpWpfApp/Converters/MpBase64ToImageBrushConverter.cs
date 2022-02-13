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

                Stretch stretchType = Stretch.Fill;
                if(parameter is string paramStr) {
                    paramStr = paramStr.ToLower();
                    if(paramStr == "none") {
                        stretchType = Stretch.None;
                    } else if(paramStr == "uniform") {
                        stretchType = Stretch.Uniform;
                    } else if(paramStr == "uniformtofill") {
                        stretchType = Stretch.UniformToFill;
                    }
                }
                return new ImageBrush() {
                    ImageSource = bmpSrc,
                    Stretch = stretchType   
                };
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            return null;
        }
    }
}
