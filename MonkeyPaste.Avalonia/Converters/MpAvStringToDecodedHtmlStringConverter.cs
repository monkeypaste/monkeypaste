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
            char[] filter = default;
            if (parameter is string paramStr &&
                paramStr.SplitNoEmpty("|") is { } paramParts &&
                paramParts.SelectMany(x => MpStringExtensions.HtmlEntityLookup.Where(y => y.Value == $"&{x};").Select(y => y.Key)).ToArray() is { } entityFilter) {
                // NOTE to workaround special entity issues, paramFilter should be
                // the encoded entity text only no leading '&' or trailing ';'
                // so to filter <, ,> it'd be 'lt|nbsp|gt'
                filter = entityFilter;
            }
            return strVal.DecodeSpecialHtmlEntities(filter);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }
    }
}
