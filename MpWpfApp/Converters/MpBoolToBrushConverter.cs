using System;
using System.Globalization;
using System.Linq;
using System.Windows.Data;

namespace MpWpfApp {
    public class MpBoolToBrushConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            if (value == null || parameter == null) {
                return null;
            }

            bool boolVal = (bool)value;
            string[] brushHexParts = parameter.ToString().Split(new string[] { "|" }, StringSplitOptions.RemoveEmptyEntries);

            if (brushHexParts.Length == 0) {
                return null;
            }

            string brushHex = brushHexParts[0];

            if (brushHexParts.Length == 2) {
                brushHex = boolVal ? brushHexParts[0] : brushHexParts[1];
            }


            return brushHex.ToSolidColorBrush();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            return null;
        }
    }
}
