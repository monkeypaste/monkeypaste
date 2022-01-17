using System;
using System.Globalization;
using System.Windows.Data;

namespace MpWpfApp {

    public class MpSourceIdToImageSourceConverter : IValueConverter {
        //returns primary source by default but secondary w/ parameter of 'SecondarySource' 
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {            
            if (value is int sourceId) {
                var svm = MpSourceCollectionViewModel.Instance.GetSourceViewModelBySourceId(sourceId);
                if(svm == null) {
                    return null;
                }
                string paramStr = string.Empty;
                MpIconViewModel ivm = null;
                if(parameter is string) {
                    paramStr = parameter as string;
                    if(paramStr.ToLower() == "border") {
                        ivm = svm.PrimarySourceIconViewModel;
                    } else { 
                        ivm = svm.SecondarySourceIconViewModel;
                    }
                } else {
                    ivm = svm.PrimarySourceIconViewModel;
                }
                if(ivm == null) {
                    return null;
                }
                return paramStr.ToLower() == "border" ? ivm.IconBorderBitmapSource : ivm.IconBitmapSource;
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            return null;
        }
    }
}
