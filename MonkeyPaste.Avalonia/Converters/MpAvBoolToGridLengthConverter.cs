using Avalonia.Controls;
using Avalonia.Data.Converters;
using MonkeyPaste.Common;
using System;
using System.Globalization;

namespace MonkeyPaste.Avalonia {
    public class MpAvBoolToGridLengthConverter : IValueConverter {
        public static readonly MpAvBoolToGridLengthConverter Instance = new();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            if (value is bool boolVal &&
                parameter is string paramStr &&
                paramStr.SplitNoEmpty("|") is string[] paramParts) {
                int idx = boolVal ? 0 : 1;
                string param_val = paramParts[idx];
                if (param_val.ToLower().EndsWith("*")) {
                    if (param_val == "*") {
                        return GridLength.Star;
                    }
                    double star_val = double.Parse(param_val.Replace("*", string.Empty));
                    return new GridLength(star_val, GridUnitType.Star);
                }
                if (param_val.ToLower() == "auto") {
                    return GridLength.Auto;
                }
                double pixel_val = double.Parse(param_val);
                return new GridLength(pixel_val, GridUnitType.Pixel);
            }
            // throw new Exception("error converting bool to grid length");
            return new GridLength(0, GridUnitType.Pixel);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            //if(paramValue is GridLength gl) {
            //    return gl.Value == 0 && gl.GridUnitType == GridUnitType.Pixel;
            //}
            //return false;
            return true;
        }
    }
}
