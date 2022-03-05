using System;
using System.Globalization;
using System.Threading.Tasks;
using System.Windows.Data;

namespace MpWpfApp {
    public class MpCopyItemIdToReportCollectionConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            if(value == null) {
                return null;
            }
            int copyItemId = (int)value;
            var civm = MpClipTrayViewModel.Instance.GetContentItemViewModelById(copyItemId);
            var arcvm = new MpAnalysisReportCollectionViewModel(civm);
            MpHelpers.RunOnMainThread(async () => {
                await arcvm.InitializeAsync(copyItemId);
            });
            return arcvm;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            return null;
        }
    }

}
