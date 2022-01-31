using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using MonkeyPaste;

namespace MpWpfApp {
    public class MpDoubleToThicknessIndentLevelConverter : IMultiValueConverter {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture) {
            if (values == null || values.Length < 2 || values[0].IsUnsetValue() || values[1].IsUnsetValue()) {
                return new Thickness();
            }
            
            int indentLevel = System.Convert.ToInt32(values[0].ToString());
            double amt = System.Convert.ToDouble(values[1].ToString()) * indentLevel;

            Thickness thickness = new Thickness();
            if(values.Length < 3) {
                thickness.Left = amt;
                return thickness;
            }
            for (int i = 2; i < values.Length; i++) {
                string val = values[i].ToString();
                if(val.ToLower() == "all") {
                    return new Thickness(amt, amt, amt, amt);
                }
                if (val.ToLower() == "left") {
                    thickness.Left = amt;
                    i++;
                } else if (val.ToLower() == "top") {
                    thickness.Top = amt;
                    i++;
                } else if (val.ToLower() == "right") {
                    thickness.Right = amt;
                    i++;
                } else if (val.ToLower() == "bottom") {
                    thickness.Bottom = amt;
                    i++;
                }
            }
            return thickness;
        }
        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }
    }
}
