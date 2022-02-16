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
            if(parameter == null) {
                throw new Exception("Must have parameter of enum type to convert back");
            }
            int intVal = System.Convert.ToInt32(value.ToString());
            string paramStr = parameter.ToString();
            return Enum.Parse(typeof(MpDb).Assembly.GetType(paramStr), intVal.ToString());

            //string enumTypeStr = parameter as string;
            //if(string.IsNullOrEmpty(enumTypeStr) || (int)value < 0) {
            //    return (int)value;
            //}
            //var enumType = Assembly.GetAssembly(typeof(MpDb)).GetType(enumTypeStr);

            //var result = Enum.ToObject(enumType, (int)value);
            //return result;
        }
    }
}
