using Avalonia.Data.Converters;
using MonkeyPaste.Common;
using System;
using System.Globalization;
using System.Linq;

namespace MonkeyPaste.Avalonia {
    public class MpAvShortcutTypeOrCommandObjToKeyStringConverter : IValueConverter {
        public static readonly MpAvShortcutTypeOrCommandObjToKeyStringConverter Instance = new();

        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture) {
            MpShortcutType sct = MpShortcutType.None;
            if (parameter is string paramStr &&
                paramStr.ToEnum<MpShortcutType>() is MpShortcutType sct_arg) {
                sct = sct_arg;
            }
            if(!sct.IsUserDefinedShortcut() && sct != MpShortcutType.None) {
                // avoid getting custom shortcut (when model has one) and bound to some application command
                if (MpAvShortcutCollectionViewModel.Instance.Items.FirstOrDefault(x => x.ShortcutType == sct) is MpAvShortcutViewModel app_cmd_svm) {
                    return app_cmd_svm.KeyString;
                }
            }

            if (value is MpIShortcutCommandViewModel scvm) {
                string custom_keystr = MpAvShortcutCollectionViewModel.Instance.GetViewModelCommandShortcutKeyString(scvm);
                if (!string.IsNullOrEmpty(custom_keystr)) {
                    return custom_keystr;
                }
            }
            if (MpAvShortcutCollectionViewModel.Instance.Items.FirstOrDefault(x => x.ShortcutType == sct) is MpAvShortcutViewModel svm) {
                // NOTE probably dont need this anymore since using pre param check but leaving since there's too many cases to test...
                return svm.KeyString;
            }
            return string.Empty;
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) {
            throw new NotSupportedException();
        }
    }
}
