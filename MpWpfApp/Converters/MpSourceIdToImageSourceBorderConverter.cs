using System;
using System.Globalization;
using System.Windows.Data;

namespace MpWpfApp {
    public class MpSourceIdToImageSourceBorderConverter : IValueConverter {
        //returns primary source by default but secondary w/ parameter of 'SecondarySource' 
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            if (value is int sourceId) {
                var svm = MpSourceCollectionViewModel.Instance.GetSourceViewModelBySourceId(sourceId);
                if (svm == null) {
                    return null;
                }
                if (parameter is string paramStr) {
                    if (paramStr == "SecondarySource") {
                        return new MpIconIdToImageSourceConverter().Convert(svm.SecondarySource.IconId,targetType,"border",culture);
                    }
                }
                return new MpIconIdToImageSourceConverter().Convert(svm.PrimarySource.IconId, targetType, "border", culture);
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            return null;
        }
    }
}
