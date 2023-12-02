using Avalonia.Data.Converters;
using MonkeyPaste.Common;
using System;
using System.Globalization;

namespace MonkeyPaste.Avalonia {
    public class MpAvStringTextToHtmlDocTextConverter : IValueConverter {
        public static readonly MpAvStringTextToHtmlDocTextConverter Instance = new();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            if (value is not string valStr) {
                return value;
            }
            if (!valStr.IsStringHtmlDocument()) {
                valStr = valStr.ToHtmlDocumentFromTextOrPartialHtml();
            }
            // strip newlines
            string result = valStr.ReplaceLineEndings(string.Empty);
            return result;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }
    }
}
