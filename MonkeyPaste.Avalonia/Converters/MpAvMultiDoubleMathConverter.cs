using Avalonia.Data.Converters;
using MonkeyPaste.Common;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;

namespace MonkeyPaste.Avalonia {
    public class MpAvMultiDoubleMathConverter : IMultiValueConverter {
        public static readonly MpAvMultiDoubleMathConverter Instance = new();

        public object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture) {
            if (values != null) {
                string[] ops;
                if (parameter is string paramStr) {
                    ops = paramStr.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);
                } else {
                    ops = Enumerable.Repeat("+", values.Count - 1).ToArray();
                }
                double outVal = 0;
                for (int i = 1; i < values.Count; i++) {
                    double a = i == 1 ? (double)values[i - 1] : outVal;
                    double b = (double)values[i];

                    string op = ops[i - 1];
                    switch (op) {
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
    public class MpAvMultiDoubleMathExpressionConverter : IMultiValueConverter {
        private static DataTable _DataTable;
        public static readonly MpAvMultiDoubleMathExpressionConverter Instance = new();

        public object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture) {
            if (values == null || values.Count == 0 ||
                parameter == null || string.IsNullOrWhiteSpace(parameter.ToString())) {
                return 0;
            }
            List<double> vals = new List<double>();
            foreach (var val in values) {
                if (val.IsUnsetValue()) {
                    vals.Add(0);
                }
                if (val is double dblVal) {
                    if (dblVal.IsNumber()) {
                        vals.Add(dblVal);
                    } else {
                        vals.Add(0);
                    }
                }
            }
            var exp = parameter.ToString();
            if (vals == null || vals.Count == 0 || string.IsNullOrWhiteSpace(exp)) {
                return 0;
            }
            return Evaluate(exp, vals.ToArray());
        }

        private double Evaluate(string exp, double[] values) {
            // exp: a/(b-c)
            exp = exp.ToLower();
            for (int i = 0; i < values.Length; i++) {
                char varName = System.Convert.ToChar((System.Convert.ToInt32('a') + i));
                exp = exp.Replace(varName.ToString(), values[i].ToString());
            }
            for (int i = 0; i < exp.Length; i++) {
                // cleanup missing values (i think when data context is null)
                if (exp[i] >= 'a' && exp[i] <= 'z') {
                    exp = exp.Replace(exp[i], '0');
                }
            }
            if (_DataTable == null) {
                _DataTable = new DataTable();
            }

            object result = null;
            try {

                result = _DataTable.Compute(exp, string.Empty);

            }
            catch (DivideByZeroException evex) {
                MpConsole.WriteTraceLine($"Error evaluating exp: '{exp}' with values: '{string.Join(",", values)}'", evex);
                result = 0.0d;
            }
            try {
                double resultVal = System.Convert.ToDouble(result);
                if (resultVal.IsNumber()) {
                    return resultVal;
                }
                return 0;

            }
            catch (Exception ex) {

                MpConsole.WriteTraceLine($"Error evaluating exp: '{exp}' with values: '{string.Join(",", values)}'", ex);
            }
            return 0;
            // ops: *|(-)
            // values: [0.5, 1, 0]

            //double outVal = 0;
            //for (int i = 1; i < values.Length; i++) {
            //    double a = i == 1 ? (double)values[i - 1] : outVal;
            //    double b = (double)values[i];

            //    string op = ops[i - 1];
            //    switch (op) {
            //        case "+":
            //            outVal = a + b;
            //            break;
            //        case "-":
            //            outVal = a - b;
            //            break;
            //        case "*":
            //            outVal = a * b;
            //            break;
            //        case "/":
            //            outVal = a / b;
            //            break;

            //    }
            //}
            //return outVal;
        }
    }
}
