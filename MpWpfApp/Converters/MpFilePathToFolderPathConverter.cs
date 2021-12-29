using System;
using System.Globalization;
using System.IO;
using System.Windows.Data;

namespace MpWpfApp {
    public class MpFilePathToFolderPathConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            if (value is string pathStr) {
                if(Directory.Exists(pathStr)) {
                    return pathStr;
                }
                if (File.Exists(pathStr)) {
                    return Path.GetDirectoryName(pathStr);
                }
            }
            return string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            return null;
        }
    }
}
