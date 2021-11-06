using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;

namespace MpWpfApp {
    public class MpBoolToCursorConverter : IValueConverter {
        public object Convert(object value, Type targetType,
                              object parameter, CultureInfo culture) {
            MpCursorType ct;

            if (value == null || parameter == null) {
                ct = MpCursorType.Default;
            } else {
                var cl = parameter.ToString().Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);
                if ((bool)value) {
                    ct = MpMouseViewModel.Instance.GetCursorFromString(cl[0]);
                } else {
                    ct = MpMouseViewModel.Instance.GetCursorFromString(cl[1]);
                }
            }
            
            MpMouseViewModel.Instance.CurrentCursor = ct;

            return MpMouseViewModel.Instance.GetCurrentCursor();
        }

        
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            return null;
        }
    }
}
    
