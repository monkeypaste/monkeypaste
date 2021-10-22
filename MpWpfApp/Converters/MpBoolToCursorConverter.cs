using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;

namespace MpWpfApp {
    public class MpBoolToCursorConverter : IValueConverter {
        public object Convert(object value, Type targetType,
                              object parameter, CultureInfo culture) {
            if(value == null || parameter == null) {
                return Cursors.Arrow;
            }
            var cl = parameter.ToString().Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);
            if((bool)value) {
                return GetCursorFromString(cl[0]);
            }
            return GetCursorFromString(cl[1]);
        }

        private Cursor GetCursorFromString(string text) {
            if(text.ToLower() == "wait") {
                return Cursors.Wait;
            }
            if (text.ToLower() == "arrow") {
                return Cursors.Arrow;
            }
            if (text.ToLower() == "ibeam") {
                return Cursors.IBeam;
            }
            if (text.ToLower() == "hand") {
                return Cursors.Hand;
            }
            return Cursors.Arrow;
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            return null;
        }
    }
}
    
