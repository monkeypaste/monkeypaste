using System;
using System.Globalization;
using System.Linq;
using System.Windows.Data;

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
                        return ivm.IconBorderBitmapSource;
                    }
                }
                return ivm.IconBitmapSource;
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            return null;
        }
    }
}
