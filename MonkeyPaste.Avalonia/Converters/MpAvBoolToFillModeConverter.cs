using Avalonia.Animation;
using Avalonia.Data.Converters;
using System;
using System.Globalization;

namespace MonkeyPaste.Avalonia {
    public class MpAvBoolToFillModeConverter : IValueConverter {
        public static readonly MpAvBoolToFillModeConverter Instance = new();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            if (value is bool isTrue) {
                return isTrue ? FillMode.Forward : FillMode.Backward;
            }
            return FillMode.None;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            if (value is FillMode fm) {
                return fm == FillMode.Forward;
            }
            return false;
        }
    }
    public class MpAvBoolToPlaybackDirectionConverter : IValueConverter {
        public static readonly MpAvBoolToPlaybackDirectionConverter Instance = new();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            if (value is bool isTrue) {
                return isTrue ? PlaybackDirection.Normal : PlaybackDirection.Reverse;
            }
            return PlaybackDirection.Normal;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            if (value is PlaybackDirection fm) {
                return fm == PlaybackDirection.Normal;
            }
            return false;
        }
    }
}
