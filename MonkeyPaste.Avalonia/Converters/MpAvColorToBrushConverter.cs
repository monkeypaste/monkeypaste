using Avalonia.Data.Converters;
using Avalonia.Media;
using MonkeyPaste.Common.Avalonia;
using System;
using System.Globalization;

namespace MonkeyPaste.Avalonia {
    public class MpAvColorToBrushConverter : IValueConverter {
        public static readonly MpAvColorToBrushConverter Instance = new();

        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture) {
            //double alpha = 1d;
            //if(parameter is string paramStr) {
            //    if(paramStr.ToLower() == "globalbgopacity") {
            //        alpha = Mp.Services.PlatformResource.GetResource<double>(MpThemeResourceKey.GlobalBgOpacity.ToString);
            //    } else if(paramStr.ToLower() == "globalbgopacity") {
            //        alpha = Mp.Services.PlatformResource.GetResource<double>(MpThemeResourceKey.GlobalBgOpacity.ToString);
            //    }
            //}
            if (value is Color c) {
                return c.ToSolidColorBrush();
            }
            return value;
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) {
            throw new NotSupportedException();
        }
    }
}
