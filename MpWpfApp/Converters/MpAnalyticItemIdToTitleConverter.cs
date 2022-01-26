using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Linq;

namespace MpWpfApp {
    public class MpAnalyticItemIdToTitleConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            var aivm = MpAnalyticItemCollectionViewModel.Instance.Items.FirstOrDefault(x => x.AnalyticItemId == (int)value);
            if(aivm == null) {
                return null;
            }
            return aivm.Title;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            return (Visibility)value == Visibility.Visible ? true : false;
        }
    }
}
