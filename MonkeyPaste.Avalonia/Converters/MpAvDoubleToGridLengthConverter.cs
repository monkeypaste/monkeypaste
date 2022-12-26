using Avalonia;
using Avalonia.Controls;
using Avalonia.Data.Converters;
using System;
using System.Globalization;
using MonkeyPaste.Common;
using System.Diagnostics;

namespace MonkeyPaste.Avalonia {
    public class MpAvDoubleToGridLengthConverter : IValueConverter {
        public static readonly MpAvDoubleToGridLengthConverter Instance = new();

        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture) {
            GridUnitType gut = parameter == null ? GridUnitType.Pixel : (GridUnitType)Enum.Parse(typeof(GridUnitType), parameter.ToString());
            if (value is double valDbl) {
                valDbl = Math.Max(0, valDbl);
                return new GridLength(valDbl, gut);
            }
            return new GridLength();
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) {
            if (value is GridLength gl) {
                if(!gl.Value.IsNumber()) {
                    Debugger.Break();
                }
                return gl.Value;
            }
            return 0;
        }
    }
}
