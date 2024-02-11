using Avalonia.Data.Converters;
using Avalonia.Media;
using System;
using System.Globalization;

namespace MonkeyPaste.Avalonia {
    public class MpAvBoolToFlowDirConverter : IValueConverter {
        public static readonly MpAvBoolToFlowDirConverter Instance = new();

        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture) {
            if (value is not bool boolVal) {
                return FlowDirection.LeftToRight;
            }
            return boolVal ? FlowDirection.RightToLeft : FlowDirection.LeftToRight;
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) {
            if (value is not FlowDirection fd) {
                return false;
            }
            return fd == FlowDirection.RightToLeft;
        }
    }
}
