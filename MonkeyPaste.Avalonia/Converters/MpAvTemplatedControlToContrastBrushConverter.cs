using Avalonia.Data.Converters;
using Avalonia.Media;
using Avalonia.VisualTree;
using MonkeyPaste.Common;
using System;
using System.Globalization;
using System.Windows.Controls;

namespace MonkeyPaste.Avalonia {
    public class MpAvTemplatedControlToContrastBrushConverter : IValueConverter {
        public static readonly MpAvTemplatedControlToContrastBrushConverter Instance = new();

        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture) {
            if (value is Control c) {
                while (c != null) {
                    if (c.Background != null) {
                        return MpAvBrushToContrastBrushConverter.Instance.Convert(c.Background, targetType, parameter, culture);
                    }
                    if (c.Parent is Control pc) {
                        c = pc;
                    } else {
                        c = null;
                    }
                }
            }
            return Mp.Services.PlatformResource.GetResource<IBrush>(MpThemeResourceKey.ThemeInteractiveColor.ToString());
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }
    }
}
