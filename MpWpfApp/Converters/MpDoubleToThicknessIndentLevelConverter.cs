using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace MpWpfApp {
    public class MpDoubleToThicknessIndentLevelConverter : IMultiValueConverter {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture) {
            if (values == null || values.Length < 2) {
                return new Thickness();
            }
            int indentLevel = System.Convert.ToInt32(parameter.ToString());
            Thickness thickness = new Thickness();
            for (int i = 0; i < values.Length; i++) {
                string val = values[i].ToString();
                if(val.ToLower() == "all") {
                    double amt = System.Convert.ToDouble(values[i + 1].ToString()) * indentLevel;
                    return new Thickness(amt, amt, amt, amt);
                }
                if (val.ToLower() == "left") {
                    double amt = System.Convert.ToDouble(values[i + 1].ToString()) * indentLevel;
                    thickness.Left = amt;
                    i++;
                } else if (val.ToLower() == "top") {
                    double amt = System.Convert.ToDouble(values[i + 1].ToString()) * indentLevel;
                    thickness.Top = amt;
                    i++;
                } else if (val.ToLower() == "right") {
                    double amt = System.Convert.ToDouble(values[i + 1].ToString()) * indentLevel;
                    thickness.Right = amt;
                    i++;
                } else if (val.ToLower() == "bottom") {
                    double amt = System.Convert.ToDouble(values[i + 1].ToString()) * indentLevel;
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
