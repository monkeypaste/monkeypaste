using Avalonia.Data.Converters;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Plugin;
using System;
using System.Globalization;

namespace MonkeyPaste.Avalonia {
    public class MpAvDateTimeToStringConverter : IValueConverter {
        public static readonly MpAvDateTimeToStringConverter Instance = new();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            if (value is DateTime dt) {
                if (parameter.ToStringOrEmpty() == "dateandtime") {
                    parameter = MpAvCurrentCultureViewModel.Instance.DateTimeFormat;
                }
                string format = parameter is string ? parameter as string : MpAvCurrentCultureViewModel.Instance.DateFormat;
                try {

                    return dt.ToString(format, UiStrings.Culture);
                }
                catch (Exception ex) {
                    MpConsole.WriteTraceLine($"Error converting datetime '{dt}' to format '{format}' for culture '{UiStrings.Culture}'. Using default", ex);
                    return dt.ToString();
                }
            }
            return null;
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) {
            throw new NotSupportedException();
        }
    }
}
