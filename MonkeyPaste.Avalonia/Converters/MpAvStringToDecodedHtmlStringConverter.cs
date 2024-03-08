using Avalonia.Data.Converters;
using MonkeyPaste.Common;
using System;
using System.Globalization;
using System.Linq;
namespace MonkeyPaste.Avalonia {
    public class MpAvStringToDecodedHtmlStringConverter : IValueConverter {
        public static MpAvStringToDecodedHtmlStringConverter Instance = new MpAvStringToDecodedHtmlStringConverter();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            if (value is not string strVal) {
                return string.Empty;
            }
            return strVal.DecodeSpecialHtmlEntities();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }
    }
}
