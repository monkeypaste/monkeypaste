using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using MonkeyPaste;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Wpf;
namespace MpWpfApp {
    public class MpStringToSizeConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            if (value == null) {
                return 0;
            }
            
            if(value is string valueStr) {
                Size outSize = new Size();
                if (parameter is string paramStr) {
                    if (valueStr.IsStringBase64()) {
                        var bmpSrc = valueStr.ToBitmapSource();
                        outSize = new Size(bmpSrc.PixelWidth, bmpSrc.PixelHeight);
                    } else {
                        var fd = valueStr.ToFlowDocument();
                        outSize = fd.GetDocumentSize();
                    } 
                    bool isWidth = paramStr.ToLower() == "width";
                    return isWidth ? outSize.Width : outSize.Height;
                }
                return outSize;
            }
            return 0;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            return null;
        }
    }
}
