using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Linq;
using System.Windows.Media.Imaging;
using System.Windows.Controls;

namespace MpWpfApp {
    public class MpAnalyticItemGuidToIconSourceConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            var aivm = MpAnalyticItemCollectionViewModel.Instance.Items.FirstOrDefault(x => x.PluginGuid == value.ToString());
            if (aivm == null) {
                return null;
            }
            return new MpIconIdToImageSourceConverter().Convert(aivm.PluginIconId,targetType,parameter,culture);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            return null;
        }
    }
}
