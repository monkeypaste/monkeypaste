using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using System.Linq;
using System.Windows.Controls;

namespace MpWpfApp {
    public class MpBoolConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            if (value == null) {
                return null;
            }
            bool isFlip = false;
            Type returnType = null;
            bool boolVal = (bool)value;
            if (parameter is string paramStr) {
                string[] paramStrParts = paramStr.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);
                foreach(var param in paramStrParts) {
                    if(param.ToLower() == "flip") {
                        isFlip = true;
                    }
                    if(param.ToLower() == "visibility") {
                        returnType = typeof(Visibility);
                    }
                    if (param.ToLower() == "brush") {
                        returnType = typeof(Brush);
                    }
                    if (param.ToLower() == "halignment") {
                        returnType = typeof(HorizontalAlignment);
                    }
                    if (param.ToLower() == "valignment") {
                        returnType = typeof(VerticalAlignment);
                    }
                    if (param.ToLower() == "sbvisibility") {
                        returnType = typeof(ScrollBarVisibility);
                    }
                }
            }
            
            return !(bool)value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            return (Visibility)value == Visibility.Visible ? true : false;
        }
    }
}
