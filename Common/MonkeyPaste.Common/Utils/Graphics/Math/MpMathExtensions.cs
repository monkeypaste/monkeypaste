using System;

namespace MonkeyPaste.Common {
    public static class MpMathExtensions {
        public static bool IsNumber(this double val) {
            return !double.IsNaN(val) &&
                   !double.IsPositiveInfinity(val) &&
                   !double.IsNegativeInfinity(val);
        }

        public static bool IsFuzzyZero(this double val, double thresh = 0.001) {
            return Math.Abs(val) <= thresh;
        }

        public static double Clamp(double val, double min, double max) {
            return Math.Max(min, Math.Min(max, val));
        }

        public static int Clamp(int val, int min, int max) {
            return Math.Max(min, Math.Min(max, val));
        }

        public static byte Clamp(byte val, byte min, byte max) {
            return Math.Max(min, Math.Min(max, val));
        }

        public static long Clamp(long val, long min, long max) {
            return Math.Max(min, Math.Min(max, val));
        }

        public static double Wrap(this double val, double min, double max) {
            if (val < min) {
                return max - (min - val);
            }
            if (val > max) {
                return min + (val - max);
            }
            return val;
        }
    }
}
