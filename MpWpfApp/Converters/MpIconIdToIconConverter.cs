using System;
using System.Globalization;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace MpWpfApp {

    public class MpIconIdToIconConverter : IValueConverter {
        //returns primary source by default but secondary w/ parameter of 'SecondarySource' 
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            BitmapSource bmpSrc = null;
            if (value is int iconId) {
                var ivm = MpIconCollectionViewModel.Instance.IconViewModels.FirstOrDefault(x => x.IconId == iconId);

                if (ivm == null) {
                    bmpSrc = null;
                } else if (parameter is string paramStr) {
                    if (paramStr.ToLower() == "border") {
                        bmpSrc = (BitmapSource)new MpBase64StringToBitmapSourceConverter().Convert(ivm.IconBorderImage.ImageBase64, null, null, CultureInfo.CurrentCulture);
                    } 
                }
                if(ivm != null) {
                    bmpSrc = (BitmapSource)new MpBase64StringToBitmapSourceConverter().Convert(ivm.IconImage.ImageBase64, null, null, CultureInfo.CurrentCulture);
                }
                
            }
            return new Image() { 
                Source = bmpSrc 
            };
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            return null;
        }
    }
}
