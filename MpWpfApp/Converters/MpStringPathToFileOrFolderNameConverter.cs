using System;
using System.Globalization;
using System.IO;
using System.Windows.Data;
using MonkeyPaste;

namespace MpWpfApp {
    public class MpStringPathToFileOrFolderNameConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            if (value is string pathStr) {
                if(pathStr.IsStringFileOrPathFormat()) {
                    //if (Directory.Exists(pathStr)) {
                    //    return Path.GetFileName(pathStr);
                    //}
                    //if (File.Exists(pathStr)) {
                    //    return Path.GetFileName(pathStr);
                    //}
                    return Path.GetFileName(pathStr);
                }
                return pathStr;
            }
            return string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            return null;
        }
    }
}
