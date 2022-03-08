using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media.Imaging;
using MonkeyPaste;
using System.Windows.Media;

namespace MpWpfApp {
    public class MpStringResourceToImageBrushConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            if (parameter is string paramStr) {
                UriKind kind = UriKind.Absolute;

                if (!paramStr.IsStringResourcePath()) {
                    paramStr = @"/Resources" + value.ToString();
                    kind = UriKind.Relative;
                } 

                Stretch stretchType = Stretch.Fill;
                //if (parameter is string paramStr) {
                //    paramStr = paramStr.ToLower();
                //    if (paramStr == "none") {
                //        stretchType = Stretch.None;
                //    } else if (paramStr == "uniform") {
                //        stretchType = Stretch.Uniform;
                //    } else if (paramStr == "uniformtofill") {
                //        stretchType = Stretch.UniformToFill;
                //    }
                //}
                return new ImageBrush() {
                    ImageSource = new BitmapImage(new Uri(paramStr,kind)),
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
