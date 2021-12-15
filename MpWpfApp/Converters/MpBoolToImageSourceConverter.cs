using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace MpWpfApp {
    public class MpBoolToImageSourceConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            if(value == null || parameter == null) {
                return null;
            }

            bool boolVal = (bool)value;

            string[] imgPaths = parameter.ToString().Split(new string[] { "|" }, StringSplitOptions.RemoveEmptyEntries);

            if(imgPaths.Length == 0) {
                return null;
            }

            string imgPath = imgPaths[0];

            if(imgPaths.Length == 2) {
                imgPath = boolVal ? imgPaths[0] : imgPaths[1];
            }

            if(!Application.Current.Resources.Contains(imgPath)) {
                throw new Exception("App.xaml Resource not found key: " + imgPath);
            }

            return Application.Current.Resources[imgPath] as string;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            return null;
        }
    }
}
