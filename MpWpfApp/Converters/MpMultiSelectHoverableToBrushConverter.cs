using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using MonkeyPaste;
using Newtonsoft.Json.Linq;

namespace MpWpfApp {
    public class MpMultiSelectHoverableToBrushConverter : IMultiValueConverter {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture) {
            if (values == null) {
                return null;
            }
            if (!values[0].IsUnsetValue() && (bool)values[0]) {
                return MpSystemColors.IsSelectedBorderColor.ToSolidColorBrush();
            }
            if (!values[1].IsUnsetValue() && (bool)values[1]) {
                return MpSystemColors.IsHoveringBorderColor.ToSolidColorBrush();
            }
            return Brushes.Transparent;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) {
            return null;
        }
    }
}
