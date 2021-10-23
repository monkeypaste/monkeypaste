using System;
using System.Globalization;
using System.IO;
using System.Web.UI;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace MpWpfApp {
    public class MpAbsoluteToRelativePathStringConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            if (value is string pathStr) {
                if(string.IsNullOrEmpty(pathStr)) {
                    return string.Empty;
                }   
                var uri = new Uri(pathStr, UriKind.Absolute);
                return Path.GetFileName(uri.LocalPath);
            }
            return new Image();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            return null;
        }
    }
}
