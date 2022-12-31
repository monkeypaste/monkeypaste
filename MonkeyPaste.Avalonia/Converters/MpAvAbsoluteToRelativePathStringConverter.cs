using Avalonia.Data.Converters;
using System;
using System.Globalization;
using System.IO;

namespace MonkeyPaste.Avalonia {
    public class MpAvAbsoluteToRelativePathStringConverter : IValueConverter {
        public static readonly MpAvAbsoluteToRelativePathStringConverter Instance = new();
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            if (value is string pathStr) {
                if (string.IsNullOrEmpty(pathStr)) {
                    return string.Empty;
                }
                var uri = new Uri(pathStr, UriKind.Absolute);
                return Path.GetFileName(uri.LocalPath);
            }
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            return null;
        }
    }
}
