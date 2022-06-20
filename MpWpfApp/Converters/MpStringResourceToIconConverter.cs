using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using MonkeyPaste;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Wpf;
namespace MpWpfApp {
    public class MpStringResourceToIconConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            if (value is string valueStr) {
                if(string.IsNullOrEmpty(valueStr)) {
                    return new Image();
                }
                if(!valueStr.IsStringResourcePath()) {
                    if(!Application.Current.Resources.Contains(valueStr)) {
                        valueStr = "QuestionMarkIcon";
                    }
                    valueStr = Application.Current.Resources[valueStr] as string;
                }
                var bmpSrc = (BitmapSource)new BitmapImage(new Uri(valueStr));
                
                if(parameter is string paramStr) {
                    if(string.IsNullOrEmpty(paramStr)) {
                        //do nothing
                    } else if(!paramStr.IsStringHexColor()) {
                        bmpSrc = bmpSrc.Tint(paramStr.ToWinMediaColor(), true);
                    }
                }
                var icon = new Image();
                icon.Source = bmpSrc;
                icon.Source.Freeze();
                return icon;
            }
            return new Image();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            return null;
        }
    }
}
