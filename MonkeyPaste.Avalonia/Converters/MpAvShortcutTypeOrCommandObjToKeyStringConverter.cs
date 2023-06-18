using Avalonia.Data.Converters;
using MonkeyPaste.Common;
using System;
using System.Globalization;
using System.Linq;

namespace MonkeyPaste.Avalonia {
    public class MpAvShortcutTypeOrCommandObjToKeyStringConverter : IValueConverter {
        public static readonly MpAvShortcutTypeOrCommandObjToKeyStringConverter Instance = new();

        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture) {
            if (parameter is string paramStr &&
                paramStr.ToEnum<MpShortcutType>() is MpShortcutType sct &&
                MpAvShortcutCollectionViewModel.Instance.Items.FirstOrDefault(x => x.ShortcutType == sct) is MpAvShortcutViewModel svm) {
                return svm.KeyString;
            }

            if (value is MpIShortcutCommandViewModel scvm) {
                return MpAvShortcutCollectionViewModel.Instance.GetViewModelCommandShortcutKeyString(scvm);
            }
            return string.Empty;
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) {
            throw new NotSupportedException();
        }
    }
}
