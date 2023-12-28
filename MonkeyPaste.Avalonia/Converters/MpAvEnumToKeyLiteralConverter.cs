using Avalonia.Data.Converters;
using MonkeyPaste.Common;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;

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
                        kl3 = kl3.ToProperCase();
                    }
                    if (Regex.IsMatch(kl3, "D[0-9]")) {
                        // hide D on digits
                        return kl3.Substring(1);
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
