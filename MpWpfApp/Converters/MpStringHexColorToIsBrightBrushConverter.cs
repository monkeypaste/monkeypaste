using MonkeyPaste;
using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace MpWpfApp {
    public class MpStringHexColorToIsBrightBrushConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            if (value.GetType() != typeof(string) || !value.ToString().IsStringHexColor()) {
                return Brushes.Transparent;
            }
            var b = (Brush)new BrushConverter().ConvertFrom(
                    MpColorHelpers.IsBright(value.ToString()) ? 
                        MpSystemColors.black : 
                        MpSystemColors.White);
            b.Freeze();
            return b;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            return value.ToString();
        }
    }
}
