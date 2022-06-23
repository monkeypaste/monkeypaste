using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using MonkeyPaste;
namespace MpWpfApp {
    public class MpEnumToImageSourceConverter : IValueConverter {
        private static MpEnumToImageResourceKeyConverter _enumToResoureKeyConverter;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            if (value is Enum valEnum) {
                if(_enumToResoureKeyConverter == null) {
                    _enumToResoureKeyConverter = new MpEnumToImageResourceKeyConverter();
                }
                string resourceKey = _enumToResoureKeyConverter.Convert(valEnum, null, null, null) as string;
                return Application.Current.Resources[resourceKey] as string;
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            return null;
        }
    }
}
