using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Data.Converters;
using MonkeyPaste.Common;
using System;
using System.Globalization;
using System.Linq;

namespace MonkeyPaste.Avalonia {
    public class MpAvStringToAvDoubleListConverter : IValueConverter {
        public static readonly MpAvStringToAvDoubleListConverter Instance = new();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            if (value is string valueStr && !string.IsNullOrEmpty(valueStr)) {
                try {
                    var result = new AvaloniaList<double>(valueStr.SplitNoEmpty(",").Select(x => double.Parse(x)));
                    return result;
                }
                catch (Exception ex) {
                    MpConsole.WriteTraceLine($"Error parsing dash array, str: '{valueStr}'", ex);
                }
            }

            return new AvaloniaList<double>();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }
    }
}
