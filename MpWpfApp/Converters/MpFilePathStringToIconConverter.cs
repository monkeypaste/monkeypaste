using System;
using System.Globalization;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace MpWpfApp {

    public class MpFilePathStringToIconConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            if (value is string pathStr) {
                if(string.IsNullOrEmpty(pathStr)) {
                    return new Image();
                }
                return MpHelpers.GetIconImage(pathStr);
            }
            return new Image();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            return null;
        }
    }
}
