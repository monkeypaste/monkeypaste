using System;

namespace MonkeyPaste.Common {
    public static class MpMathExtensions {
        public static bool IsNumber(this double val) {
            return !double.IsNaN(val) &&
                   !double.IsPositiveInfinity(val) &&
                   !double.IsNegativeInfinity(val);
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
    }
}
