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
                if(parameter is string paramStr) {
                    if(paramStr == "SecondarySource") {

                        return svm.SecondarySourceIconViewModel.IconBitmapSource;
                    }
                }
                return svm.PrimarySourceIconViewModel.IconBitmapSource;
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            return null;
        }
    }
}
