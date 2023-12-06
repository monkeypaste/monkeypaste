using Avalonia.Data.Converters;
using MonkeyPaste.Common;
using System;
using System.Globalization;

namespace MonkeyPaste.Avalonia {
    public class MpAvEnumKeyToUiStringConverter : IValueConverter {
        public static readonly MpAvEnumKeyToUiStringConverter Instance = new();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            if (parameter is not string paramStr) {
                return string.Empty;
            }
            return MpAvEnumUiStringExtensions.EnumKeyToUiString(paramStr);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }
    }
}
