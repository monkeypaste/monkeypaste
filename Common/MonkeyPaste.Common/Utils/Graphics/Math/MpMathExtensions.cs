using System;
using System.Collections.Generic;
using System.Text;

namespace MonkeyPaste.Common {
    public static class MpMathExtensions {
        public static bool IsNumber(this double val) {
            return !double.IsNaN(val) &&
                   !double.IsPositiveInfinity(val) &&
                   !double.IsNegativeInfinity(val);
        }
    }
}
