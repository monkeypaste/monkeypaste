using System;
using System.Globalization;
using System.Windows.Data;
using System.Linq;
using System.Windows.Input;
using MonkeyPaste;

namespace MpWpfApp {
    public class MpShortcutCommandViewModelToTooltipConverter : IValueConverter {
        //returns primary source by default but secondary w/ parameter of 'SecondarySource' 
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            if (value is MpIShortcutCommandViewModel sccvm) {
                string shortcutKeyString = MpDataModelProvider.GetShortcutKeystring(sccvm.ShortcutType.ToString(), sccvm.ModelId.ToString());
                if(string.IsNullOrEmpty(shortcutKeyString)) {
                    return "Assign " + sccvm.ShortcutLabel + " Shortcut";
                }
                return sccvm.ShortcutLabel + " " + shortcutKeyString;
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            return null;
        }
    }
}
