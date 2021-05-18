using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using Xamarin.Forms;

namespace MonkeyPaste {
    public class MpByteArrayToImageSourceConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value != null)
            {
                var bytes = (byte[])value;
                using (var stream = new MemoryStream(bytes))
                {
                    return ImageSource.FromStream(() => stream);
                }
            }
            return null;
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }

}
    
