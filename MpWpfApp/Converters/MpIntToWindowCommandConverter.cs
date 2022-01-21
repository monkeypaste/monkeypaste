using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace MpWpfApp {
    public class MpIntToWindowCommandConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            return (MpProcessHelper.WinApi.ShowWindowCommands)((int)value);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            return (int)((MpProcessHelper.WinApi.ShowWindowCommands)value);
        }
    }
}
