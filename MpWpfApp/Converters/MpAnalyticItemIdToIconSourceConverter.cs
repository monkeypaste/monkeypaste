using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Linq;

namespace MpWpfApp {
    public class MpAnalyticItemIdToIconSourceConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            var aivm = MpAnalyticItemCollectionViewModel.Instance.Items.FirstOrDefault(x => x.AnalyzerPluginSudoId == (int)value);
            if (aivm == null) {
                return null;
            }
            return new MpIconIdToImageSourceConverter().Convert(aivm.IconId,targetType,parameter,culture);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            return null;
        }
    }
}
