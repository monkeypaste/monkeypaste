using System;
using System.Globalization;
using System.Windows.Data;
using System.Linq;
using MonkeyPaste.Plugin;

namespace MpWpfApp {
    public class MpParameterFormatToItemsSourceConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            if(value == null) {
                return null;
            }
            var paramFormat = value as MpPluginParameterFormat;

            return paramFormat.values.Select(x => x.label).ToList();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            return null;
        }
    }
}
