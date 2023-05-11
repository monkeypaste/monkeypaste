using Avalonia.Data.Converters;
using MonkeyPaste.Common;
using System;
using System.Globalization;
using System.Linq;

namespace MonkeyPaste.Avalonia {
    public class MpAvShortcutTypeToKeyStringConverter : IValueConverter {
        public static readonly MpAvShortcutTypeToKeyStringConverter Instance = new();

        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture) {
            // NOTE only works for app shortcuts
            if (parameter is string paramStr &&
                paramStr.ToEnum<MpShortcutType>() is MpShortcutType sct &&
                MpAvShortcutCollectionViewModel.Instance.Items.FirstOrDefault(x => x.ShortcutType == sct) is MpAvShortcutViewModel scvm) {
                return scvm.KeyString;
            }
            return string.Empty;
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) {
            throw new NotSupportedException();
        }
    }
}
