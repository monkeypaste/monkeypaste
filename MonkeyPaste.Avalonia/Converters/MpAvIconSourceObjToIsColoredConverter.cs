using Avalonia.Data.Converters;
using System;
using System.Globalization;

namespace MonkeyPaste.Avalonia {
    public class MpAvIconSourceObjToIsColoredConverter : IValueConverter {
        public static readonly MpAvIconSourceObjToIsColoredConverter Instance = new(); 

        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture) {
            return MpAvThemeViewModel.Instance.IsColoredImageResource(value);
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) {
            throw new NotSupportedException();
        }
    }
}
