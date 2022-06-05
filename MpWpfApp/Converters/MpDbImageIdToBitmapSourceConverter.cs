using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media.Imaging;
using MonkeyPaste;
using MonkeyPaste.Common.Wpf;

namespace MpWpfApp {
    public class MpDbImageIdToBitmapSourceConverter : IValueConverter {
        //returns primary source by default but secondary w/ parameter of 'SecondarySource' 
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            BitmapSource bmpSrc = null;
            if (value is int dbImageId) {
                var dbImg = MpDataModelProvider.GetDbImageById(dbImageId);
                if (dbImg == null) {
                    return null;
                }
                bmpSrc = dbImg.ImageBase64.ToBitmapSource();
                bmpSrc.Freeze();
            }

            return bmpSrc;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            return null;
        }
    }
}
