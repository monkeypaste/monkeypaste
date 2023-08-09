using Avalonia.Data.Converters;
using System;
using System.Globalization;

namespace MonkeyPaste.Avalonia {
    public class MpAvStringFileOrFolderPathToBitmapConverter : IValueConverter {
        public static MpAvStringFileOrFolderPathToBitmapConverter Instance = new MpAvStringFileOrFolderPathToBitmapConverter();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            return MpAvIconSourceObjToBitmapConverter.Instance.Convert(value, targetType, parameter == null ? "pathimg" : parameter, culture);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }
    }
}
