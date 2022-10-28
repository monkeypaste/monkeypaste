using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Data.Converters;
using System;
using System.Globalization;

namespace MonkeyPaste.Avalonia {
    public class MpAvDoubleArrayToAvListConverter : IValueConverter {
        public static readonly MpAvDoubleArrayToAvListConverter Instance = new();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            AvaloniaList<double> avDoubleList = new AvaloniaList<double>();
            if(value is double[] doubleArr) {
                avDoubleList.AddRange(doubleArr);
            }
            return avDoubleList;
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) {
            throw new NotSupportedException();
        }
    }
}
