using System;
using System.Globalization;
using System.Linq;
using System.Windows.Data;
using MonkeyPaste;
using MonkeyPaste.Common;

namespace MpWpfApp {
    public class MpMultiBoolOrConverter : IMultiValueConverter {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture) {
            if (values == null || values.Length == 0) {
                return false;
            }
            return values.Where(x=>!x.IsUnsetValue()).Any(x => (bool)x);
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }
    }
}
