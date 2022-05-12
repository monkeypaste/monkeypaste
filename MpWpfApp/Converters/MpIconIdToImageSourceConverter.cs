using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows.Data;
using static OpenTK.Graphics.OpenGL.GL;

namespace MpWpfApp {
    public class MpIconIdToImageSourceConverter : IValueConverter {
        //returns primary source by default but secondary w/ parameter of 'SecondarySource' 
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            if (value is int iconId) {
                var ivm = MpIconCollectionViewModel.Instance.IconViewModels.FirstOrDefault(x => x.IconId == iconId);

                if (ivm == null) {
                    return null;
                }
                if (parameter is string paramStr) {
                    if (paramStr.ToLower() == "border") {
                        return new MpBase64StringToBitmapSourceConverter().Convert(ivm.Icon.IconBorderImage.ImageBase64, null, null, CultureInfo.CurrentCulture);
                    }
                }
                return new MpBase64StringToBitmapSourceConverter().Convert(ivm.Icon.IconImage.ImageBase64, null, null, CultureInfo.CurrentCulture);
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            return null;
        }
    }

}
