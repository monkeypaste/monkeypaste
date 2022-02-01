using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using MonkeyPaste;

namespace MpWpfApp {
    public class MpActionTypeToBrushConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            if (value == null) {
                return false;
            }
            MpActionType at = (MpActionType)value;
            switch(at) {
                case MpActionType.Trigger:
                    return Brushes.Chartreuse;
                case MpActionType.Analyze:
                    return Brushes.Magenta;
                case MpActionType.Classify:
                    return Brushes.Tomato;
                case MpActionType.Compare:
                    return Brushes.Cyan;
                case MpActionType.Macro:
                    return Brushes.LightSalmon;
                case MpActionType.Timer:
                    return Brushes.CornflowerBlue;
            }
            return Brushes.White;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            return 0;
        }
    }


}
