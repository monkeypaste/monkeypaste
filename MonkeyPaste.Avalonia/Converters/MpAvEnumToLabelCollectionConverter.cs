using Avalonia;
using Avalonia.Data.Converters;
using System;
using System.Globalization;
using MonkeyPaste;
using MonkeyPaste.Common;
using System.Collections.ObjectModel;

namespace MonkeyPaste.Avalonia {
    public class MpAvEnumToLabelCollectionConverter : IValueConverter {
        public static readonly MpAvEnumToLabelCollectionConverter Instance = new();

        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture) {
            if (parameter == null || string.IsNullOrEmpty(parameter.ToString())) {
                return null;
            }

            string enumTypeName = parameter.ToString();
            string noneLabel = "";
            bool hideFirst = false;
            if (enumTypeName.Contains("|")) {
                var paramParts = enumTypeName.Split(new string[] { "|" }, StringSplitOptions.RemoveEmptyEntries);
                enumTypeName = paramParts[0];
                noneLabel = paramParts[1];
                if(noneLabel == "omit") {
                    hideFirst = true;
                }
            }
            Type enumType;

            if (enumTypeName.Contains("MonkeyPaste.Common")) {
                enumType = typeof(MpClipboardFormatType).Assembly.GetType(enumTypeName);
            } else if (enumTypeName.Contains("MpWpfApp")) {
                enumType = typeof(MpAvMainWindow).Assembly.GetType(enumTypeName);
            } else {
                enumType = typeof(MpDb).Assembly.GetType(enumTypeName);
            }
            return new ObservableCollection<string>(enumType.EnumToLabels(noneLabel,hideFirst));
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) {
            throw new NotSupportedException();
        }
    }
}
