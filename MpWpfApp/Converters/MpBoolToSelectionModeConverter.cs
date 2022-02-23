using System;
using System.Globalization;
using System.Windows.Controls;
using System.Windows.Data;

namespace MpWpfApp {
    public class MpBoolToSelectionModeConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            if (value == null) {
                return SelectionMode.Single;
            }
            bool boolVal = (bool)value;
            if(boolVal) {
                return SelectionMode.Multiple;
            }
            return SelectionMode.Single;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            return null;
        }
    }
}
