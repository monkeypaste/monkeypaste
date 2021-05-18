using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace MonkeyPaste {
    public class MpFontImageToImageSourceConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value != null && value is FontImageExtension fie)
            {
                //return fie.ProvideValue(IServiceProvider)
            }
            return null;
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }

}
    
