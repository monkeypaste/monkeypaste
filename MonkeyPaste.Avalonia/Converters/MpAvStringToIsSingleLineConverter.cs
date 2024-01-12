using Avalonia.Data.Converters;
using MonkeyPaste.Common;
using System;
using System.Globalization;
using System.Linq;
namespace MonkeyPaste.Avalonia {
    public class MpAvStringToIsSingleLineConverter : IValueConverter {
        public static MpAvStringToIsSingleLineConverter Instance = new MpAvStringToIsSingleLineConverter();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            if (value is not string strVal) {
                return false;
            }

            int new_line_count = strVal.QueryText(Environment.NewLine, false, false, false).Count();
            if (new_line_count == 0 ||
                (new_line_count == 1 && strVal.EndsWith(Environment.NewLine))) {
                // don't count single line trailing w/ new line as multiline
                return true;
            }
            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }
    }
}
