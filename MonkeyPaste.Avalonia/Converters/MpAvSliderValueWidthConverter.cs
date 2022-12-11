using Avalonia.Data.Converters;
using System;
using System.Globalization;

namespace MonkeyPaste.Avalonia {
    public class MpAvSliderValueWidthConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            if (value == null) {
                return 0;
            }

            var svm = value as MonkeyPaste.MpISliderViewModel;
            double percentFilled = svm.SliderValue / (svm.MaxValue - svm.MinValue);
            double width = svm.TotalWidth * percentFilled;
            return width;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            return null;
        }
    }
}
