using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Linq;

namespace MpWpfApp {
    public class MpTagIdToTagNameConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            var ttvm = MpTagTrayViewModel.Instance.Items.FirstOrDefault(x => x.TagId == (int)value);
            if (ttvm == null) {
                return null;
            }
            return ttvm.TagName;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            return null;
        }
    }
}
