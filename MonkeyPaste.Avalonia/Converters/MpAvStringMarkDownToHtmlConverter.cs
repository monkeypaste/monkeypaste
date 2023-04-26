using Avalonia.Data.Converters;
using Markdig;
using System;
using System.Globalization;

namespace MonkeyPaste.Avalonia {
    public class MpAvStringMarkDownToHtmlConverter : IValueConverter {
        public static readonly MpAvStringMarkDownToHtmlConverter Instance = new();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            // from https://github.com/xoofx/markdig
            if (value is string md_str) {
                string md_html = Markdown.ToHtml(md_str);
                return md_html;
            }
            return string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }
    }
}
