using System;
using System.Collections.Generic;
using System.Text;

namespace MonkeyPaste {
    public static class MpMathHelpers {
        public static double WrapValue(double x, double x_min, double x_max) {
            return ((x - x_min) % (x_max - x_min)) + x_min;
        }
    }
}
