using MonkeyPaste;
using System;
using System.Globalization;
using System.Windows.Data;

namespace MpWpfApp {

    public class MpSourceIdToImageSourceConverter : IValueConverter {
        //returns primary source by default but secondary w/ parameter of 'SecondarySource' 
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {            
            if (value is int sourceId) {
                var scvm = MpSourceCollectionViewModel.Instance;
                var svm = scvm.GetSourceViewModelBySourceId(sourceId);
                if(svm == null) {
                    return null;
                }
                return new MpIconIdToImageSourceConverter().Convert(svm.PrimarySource.SourceIcon.Id, targetType, parameter, culture);

                //string paramStr = string.Empty;
                //MpIconViewModel ivm = null;
                //if(parameter is string) {
                //    paramStr = parameter as string;
                //    if(paramStr.ToLower() == "border") {
                //        ivm = svm.PrimarySourceIconViewModel;
                //    } else { 
                //        ivm = svm.SecondarySourceIconViewModel;
                //    }
                //} else {
                //    ivm = svm.PrimarySourceIconViewModel;
                //}
                //if(ivm == null) {
                //    return null;
                //}
                //return paramStr.ToLower() == "border" ? ivm.IconBorderBitmapSource : ivm.IconBitmapSource;
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            return null;
        }
    }
}
