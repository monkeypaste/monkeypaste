using Avalonia.Data.Converters;
using MonkeyPaste.Common;
using System;
using System.Globalization;

namespace MonkeyPaste.Avalonia {
    public class MpAvEnumToUiStringConverter : IValueConverter {
        public static readonly MpAvEnumToUiStringConverter Instance = new();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            if(value is not Enum enumObj) {
                return string.Empty;
            }
            string none_text = parameter.ToStringOrEmpty();
            return enumObj.EnumToUiString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }
    }
}
