using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows.Data;
using MonkeyPaste;
using MonkeyPaste.Common.Plugin; using MonkeyPaste.Common;

namespace MpWpfApp {
    public class MpEnumToItemSourceLabelsConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            if (parameter == null || string.IsNullOrEmpty(parameter.ToString())) {
                return null;
            }

            string enumTypeName = parameter.ToString();
            string noneLabel = "";
            if(enumTypeName.Contains("|")) {
                var paramParts = enumTypeName.Split(new string[] { "|" }, StringSplitOptions.RemoveEmptyEntries);
                enumTypeName = paramParts[0];
                noneLabel = paramParts[1];
            }
            Type enumType;
            
            if(enumTypeName.Contains("MonkeyPaste.Common")) {
                enumType = typeof(MpClipboardFormatType).Assembly.GetType(enumTypeName);
            } else if(enumTypeName.Contains("MpWpfApp")) {
                enumType = typeof(MpMainWindow).Assembly.GetType(enumTypeName);
            } else {
                enumType = typeof(MpDb).Assembly.GetType(enumTypeName);
            }
            return new ObservableCollection<string>(enumType.EnumToLabels(noneLabel));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            return null;
        }
    }
}
