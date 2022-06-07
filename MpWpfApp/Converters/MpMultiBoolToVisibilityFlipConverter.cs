using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace MpWpfApp {
    public class MpMultiBoolToVisibilityFlipConverter : IMultiValueConverter {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture) {
            string op = "AND";
            if(parameter != null && parameter.ToString() == "Or") {
                op = "Or";
            }

            bool visible = true;
            foreach (object value in values) {
                if (value is bool) {
                    switch(op) {
                        case "AND":
                            visible = visible && (bool)value;
                            break;
                        case "Or":
                            visible = visible || (bool)value;
                            break;
                    }
                }
            }

            //if (parameter != null) {
            //    return !visible ? Visibility.Visible : Visibility.Hidden;
            //}
            return !visible ? Visibility.Visible : Visibility.Collapsed;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }
    }
}
