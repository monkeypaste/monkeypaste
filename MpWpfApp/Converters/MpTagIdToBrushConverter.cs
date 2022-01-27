using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Linq;

namespace MpWpfApp {
    public class MpTagIdToBrushConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            var ttvm = MpTagTrayViewModel.Instance.Items.FirstOrDefault(x => x.TagId == (int)value);
            if (ttvm == null) {
                return null;
            }
            return new MpStringHexToBrushConverter().Convert(ttvm.TagHexColor, targetType, parameter, culture);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            return (Visibility)value == Visibility.Visible ? true : false;
        }
    }
}
