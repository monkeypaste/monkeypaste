using Avalonia.Data.Converters;
using System;
using System.Globalization;
using System.Linq;

namespace MonkeyPaste.Avalonia {
    public class MpAvShortcutCommandToKeyGroupViewModelConverter : IValueConverter {
        public static readonly MpAvShortcutCommandToKeyGroupViewModelConverter Instance = new();

        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture) {
            if (value is MpAvIShortcutCommandViewModel scvm &&
                MpAvShortcutCollectionViewModel.Instance.GetViewModelCommandShortcutId(scvm) is int sid &&
                MpAvShortcutCollectionViewModel.Instance.Items.FirstOrDefault(x => x.ShortcutId == sid) is MpAvShortcutViewModel svm) {
                return svm.KeyString.ToKeyItems();
            }
            return null;
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) {
            throw new NotSupportedException();
        }
    }
}
