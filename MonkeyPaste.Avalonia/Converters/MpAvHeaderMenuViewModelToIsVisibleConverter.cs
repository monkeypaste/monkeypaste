using Avalonia.Data.Converters;
using System;
using System.Globalization;

namespace MonkeyPaste.Avalonia {
    public class MpAvHeaderMenuViewModelToIsVisibleConverter : IValueConverter {
        public static readonly MpAvHeaderMenuViewModelToIsVisibleConverter Instance = new();

        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture) {
            if(value is not MpAvIHeaderMenuViewModel hmvm) {
                return false;
            }
            if(hmvm is MpAvClipTileViewModel ctvm) {
                return !ctvm.IsAnyPlaceholder;
            }
            return true;
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }
    }
}
