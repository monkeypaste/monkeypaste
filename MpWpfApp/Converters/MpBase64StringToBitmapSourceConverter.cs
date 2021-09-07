using System;
using System.Globalization;
using System.IO;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace MpWpfApp {
    public class MpBase64StringToBitmapSourceConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            if (value is string valueStr) { 
                byte[] byteBuffer = System.Convert.FromBase64String(valueStr);

                return (BitmapSource)new ImageSourceConverter().ConvertFrom(byteBuffer);
                //using (MemoryStream memoryStream = new MemoryStream(byteBuffer)) {
                //    memoryStream.Position = 0;

                //    BitmapImage bitmapImage = new BitmapImage() {
                //        StreamSource = memoryStream
                //    };

                //    memoryStream.Close();
                //    byteBuffer = null;

                //    return bitmapImage;
                //}                    
            }
            return new Image();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            return null;
        }
    }
}
