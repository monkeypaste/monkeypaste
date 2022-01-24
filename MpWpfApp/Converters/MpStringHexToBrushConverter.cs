using MonkeyPaste;
using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace MpWpfApp {
    public class MpStringHexToBrushConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            if(value.GetType() != typeof(string) || !value.ToString().IsStringHexColor()) {
                return Brushes.Transparent;
            }
            var b = (Brush)new BrushConverter().ConvertFrom(value.ToString());
            if(parameter is string paramStr) {
                double opacity = 1.0;
                try {
                    opacity = System.Convert.ToDouble(opacity);
                }catch(Exception ex) {
                    MpConsole.WriteTraceLine($"Couldn't convert param '{paramStr}' to a double for opacity");
                    opacity = 1.0;
                }
                b.Opacity = opacity;                
            }
            b.Freeze();
            return b;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            return value.ToString();
        }
    }
}
