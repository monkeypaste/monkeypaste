using MonkeyPaste;
using System;
using System.Globalization;
using System.Windows.Data;

namespace MpWpfApp {
    public class MpStringToAnalyticNumberConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            if(value == null || parameter.GetType() != typeof(MpAnalyticItemParameterValueUnitType)) {
                return value;
            }
            switch((MpAnalyticItemParameterValueUnitType)parameter) {
                case MpAnalyticItemParameterValueUnitType.Decimal:
                    return Math.Round(System.Convert.ToDouble(value.ToString()), 2).ToString();
                case MpAnalyticItemParameterValueUnitType.Integer:
                    return Math.Round(System.Convert.ToDouble(value.ToString()), 0).ToString();
                default:
                    return value.ToString();
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            return System.Convert.ToInt32(value.ToString());
        }
    }
}
