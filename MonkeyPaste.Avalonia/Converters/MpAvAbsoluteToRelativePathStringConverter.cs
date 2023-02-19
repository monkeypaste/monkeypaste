using Avalonia.Data.Converters;
using System;
using System.Globalization;
using System.IO;

namespace MonkeyPaste.Avalonia {
    public class MpAvAbsoluteToRelativePathStringConverter : IValueConverter {
        public static readonly MpAvAbsoluteToRelativePathStringConverter Instance = new();
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            string fallback_text = string.Empty;
            if (parameter is string param_str) {
                fallback_text = param_str;
            }
            if (value is string pathStr &&
                !string.IsNullOrEmpty(pathStr)) {
                try {
                    // convert path to uri to ensure its a path
                    var uri = new Uri(pathStr, UriKind.Absolute);
                    return Path.GetFileName(uri.LocalPath);
                }
                catch {

                }
            }
            return fallback_text;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            return null;
        }
    }
}
