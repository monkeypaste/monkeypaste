using System;
using System.Globalization;
using System.Linq;
using System.Windows.Data;
using MonkeyPaste;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Wpf;

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

            string brushHexOrNamedColorStr = brushHexParts[0];

            if (brushHexParts.Length == 2) {
                brushHexOrNamedColorStr = boolVal ? brushHexParts[0] : brushHexParts[1];
            }
            
            return MpSystemColors.ConvertFromString(brushHexOrNamedColorStr).ToWpfBrush();

            
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            return null;
        }
    }

    public class MpBoolFlipToBrushConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            if (value == null || parameter == null) {
                return null;
            }

            bool boolVal = (bool)value;
            return new MpBoolToBrushConverter().Convert(!boolVal, targetType, parameter, culture);

        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            return null;
        }
    }
}
