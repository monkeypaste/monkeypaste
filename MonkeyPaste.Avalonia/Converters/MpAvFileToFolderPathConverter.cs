using Avalonia.Data.Converters;
using MonkeyPaste.Common;
using System;
using System.Globalization;
using System.IO;

namespace MonkeyPaste.Avalonia {
    public class MpAvFileToFolderPathConverter : IValueConverter {
        public static readonly MpAvFileToFolderPathConverter Instance = new();
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            if (value is string pathStr &&
                pathStr.IsFile()) {
                return Path.GetDirectoryName(pathStr);
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            return null;
        }
    }
}
