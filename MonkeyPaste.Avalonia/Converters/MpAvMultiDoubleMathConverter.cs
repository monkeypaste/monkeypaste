using Avalonia;
using Avalonia.Data.Converters;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using MonkeyPaste.Common;
using System.Diagnostics;

namespace MonkeyPaste.Avalonia {
    public class MpAvMultiDoubleMathConverter : IMultiValueConverter {
        public static readonly MpAvMultiDoubleMathConverter Instance = new();

        public object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture) {
            if(values != null) {
                string[] ops;
                if(parameter is string paramStr) {
                    ops = paramStr.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);
                } else {
                    ops = Enumerable.Repeat("+", values.Count - 1).ToArray();
                }
                double outVal = 0;
                for (int i = 1; i < values.Count; i++) {
                    double a = i == 1 ? (double)values[i - 1] : outVal;
                    double b = (double)values[i];

                    string op = ops[i - 1];
                    switch(op) {
                        case "+":
                            outVal = a + b;
                            break;
                        case "-":
                            outVal = a - b;
                            break;
                        case "*":
                            outVal = a * b;
                            break;
                        case "/":
                            outVal = a / b;
                            break;
                    }
                }
                return outVal;
            }
            return 0;
        }
    }
}
