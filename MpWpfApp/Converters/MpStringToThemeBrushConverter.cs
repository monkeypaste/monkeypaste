using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace MpWpfApp {
    public class MpStringToThemeBrushConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {

            var valStr = parameter as string;
            return MpThemeColors.Instance.CurrentTheme[valStr];
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            return value.ToString();
        }
    }
}
