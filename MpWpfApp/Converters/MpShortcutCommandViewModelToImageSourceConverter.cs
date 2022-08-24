using System;
using System.Globalization;
using System.Windows.Data;
using MonkeyPaste;
using MonkeyPaste.Common.Wpf;

namespace MpWpfApp {
    public class MpShortcutCommandViewModelToImageSourceConverter : IValueConverter {
        //returns primary source by default but secondary w/ parameter of 'SecondarySource' 
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            if (value is MpIShortcutCommandViewModel sccvm) {
                string shortcutKeyString = MpDataModelProvider.GetShortcutKeystring(sccvm.ShortcutType.ToString(), sccvm.ModelId.ToString());
                if (string.IsNullOrEmpty(shortcutKeyString)) {
                    return MpBase64Images.JoystickUnset.ToBitmapSource();
                }
                return MpBase64Images.JoystickActive.ToBitmapSource();
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            return null;
        }
    }
}
