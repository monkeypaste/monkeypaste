
using System;

namespace MonkeyPaste {
    [Flags]
    public enum MpSearchCriteriaUnitFlags {
        None = 0,
        Text = 1,
        Hex = 512,
        Integer = 2,
        Rgba = 1024,
        Decimal = 256,
        UnitDecimal = 32768,
        //UnitDecimalX4 = 2048,
        DateTime = 128,
        TimeSpan = 8,
        Enumerable = 16,
        EnumerableValue = 4096,
        RegEx = 32,
        CaseSensitivity = 64,
        Bit = 8192,
        WholeWord = 16384
    };
    public static class MpSearchCriteriaUnitFlagExtensions {

        public static bool HasUnsignedNumeric(this MpSearchCriteriaUnitFlags scuf) {
            return
                scuf.HasFlag(MpSearchCriteriaUnitFlags.Integer) ||
                scuf.HasFlag(MpSearchCriteriaUnitFlags.Rgba) ||
                scuf.HasFlag(MpSearchCriteriaUnitFlags.Decimal);
        }
        public static Tuple<double, double> GetNumericBounds(this MpSearchCriteriaUnitFlags scuf) {
            if (!scuf.HasUnsignedNumeric()) {
                return null;
            }
            // NOTE order from smallest to largest
            if (scuf.HasFlag(MpSearchCriteriaUnitFlags.Rgba)) {
                return new Tuple<double, double>(0, 255);
            }
            if (scuf.HasFlag(MpSearchCriteriaUnitFlags.Integer)) {
                return new Tuple<double, double>(0, int.MaxValue);
            }
            if (scuf.HasFlag(MpSearchCriteriaUnitFlags.Decimal)) {
                return new Tuple<double, double>(0, double.MaxValue);
            }
            throw new Exception($"Unknown unit flags '{scuf}'");
        }

        public static bool IsInBounds(double val, Tuple<double, double> bounds) {
            if (bounds == null) {
                return true;
            }
            return val >= bounds.Item1 && val <= bounds.Item2;
        }
    }
    public enum MpTimeSpanWithinUnitType {
        None = 0,
        Hours,
        Days,
        Weeks,
        Months,
        Years
    }

    public enum MpDateBeforeUnitType {
        None = 0,
        Today,
        Yesterday,
        ThisWeek,
        ThisMonth,
        ThisYear,
        Exact,
        Startup
    }

    public enum MpDateAfterUnitType {
        None = 0,
        Yesterday,
        LastWeek,
        LastMonth,
        LastYear,
        Exact,
        Startup
    }


}