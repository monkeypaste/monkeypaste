using System;
using System.Globalization;
using System.Windows.Data;
using MonkeyPaste;
using MonkeyPaste.Common;

namespace MpWpfApp {
    public class MpShortcutTypeToStringConverter : IValueConverter {
        //returns primary source by default but secondary w/ parameter of 'SecondarySource' 
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            //if (parameter is string paramStr && Enum.TryParse(paramStr,out MpShortcutType sct)) {
            //    string shortcutKeyString = MpDataModelProvider.GetShortcutKeystring(sct);
            //    return $"{sct.EnumToLabel()} {shortcutKeyString}";
            //}
            if (parameter is MpShortcutType sct) {
                string shortcutKeyString = MpDataModelProvider.GetShortcutKeystring(sct);
                return $"{sct.EnumToLabel()} {shortcutKeyString}";
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            return null;
        }
    }
}
