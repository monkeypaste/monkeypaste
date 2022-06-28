using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media.Imaging;
using MonkeyPaste;
using MonkeyPaste.Common;

namespace MpWpfApp {
    public class MpObjectToImageSourceOrIconConverter : IValueConverter {
        //returns primary source by default but secondary w/ parameter of 'SecondarySource' 
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            bool isIcon = parameter != null;
            if (value is int iconId && iconId > 0) {
                if(isIcon) {
                    return new MpIconIdToIconConverter().Convert(value, targetType, parameter, culture);
                }
                return new MpIconIdToImageSourceConverter().Convert(value, targetType, parameter, culture);
            }
            if(value is string valueStr) {
                if (valueStr.IsStringHexColor()) {
                    if(isIcon) {
                        return new MpStringHexToIconConverter().Convert(value, targetType, parameter, culture);
                    }
                    return new MpStringHexToImageSourceConverter().Convert(value, targetType, parameter, culture);
                }
                if (valueStr.IsStringResourcePath()) {
                    if(isIcon) {
                        return new MpStringResourceToIconConverter().Convert(value, targetType, parameter, culture);
                    }
                    return new MpStringResourceToImageSourceConverter().Convert(value, targetType, parameter, culture);
                }
            } else if(value is Uri valueUri) {
                return new BitmapImage() {
                    UriSource = valueUri
                };
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            return null;
        }
    }
}
