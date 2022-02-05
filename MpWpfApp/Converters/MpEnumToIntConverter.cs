using System;
using System.Globalization;
using System.Reflection;
using System.Windows;
using System.Windows.Data;
using MonkeyPaste;

namespace MpWpfApp {
    public class MpEnumToIntConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            if (value == null) {
                return 0;
            }
            var result = (int)value;
            return result;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            if(parameter == null || value == null) {
                return (int)value;
            }
            string enumTypeStr = parameter as string;
            if(string.IsNullOrEmpty(enumTypeStr) || (int)value < 0) {
                return (int)value;
            }
            var enumType = Assembly.GetAssembly(typeof(MpDb)).GetType(enumTypeStr);

            var result = Enum.ToObject(enumType, (int)value);
            return result;
        }
    }
}
