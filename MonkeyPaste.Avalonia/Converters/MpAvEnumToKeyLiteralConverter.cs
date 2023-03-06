using Avalonia.Data.Converters;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Avalonia;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace MonkeyPaste.Avalonia {
    public class MpAvEnumToKeyLiteralConverter : IValueConverter {
        public static readonly MpAvEnumToKeyLiteralConverter Instance = new();

        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture) {
            if (value is string valueStr) {
                if (string.IsNullOrEmpty(valueStr)) {
                    return string.Empty;
                }
                //string key_literal = MpAvInternalKeyConverter.GetKeyLiteral(
                //        MpAvInternalKeyConverter.ConvertStringToKey(valueStr));
                var kl = Mp.Services.KeyConverter.ConvertStringToKeyLiteralSequence(valueStr);
                if (kl.FirstOrDefault() is IEnumerable<string> kl2 &&
                    kl2.FirstOrDefault() is string kl3) {
                    if (parameter is string paramStr && paramStr == "label") {
                        kl3 = kl3.ToLabel();
                    }
                    return kl3;
                }

            }
            return null;
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) {
            throw new NotSupportedException();
        }
    }


}
