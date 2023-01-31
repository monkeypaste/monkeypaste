
using System;

namespace MonkeyPaste.Common {
    [Flags]
    public enum MpSearchCriteriaUnitFlags {
        None = 0,
        Text = 1,
        Hex = 512,
        Integer = 2,
        ByteX4 = 1024,
        Decimal = 256,
        UnitDecimalX4 = 2048,
        DateTime = 128,
        TimeSpan = 8,
        Enumerable = 16,
        EnumerableValue = 4096,
        RegEx = 32,
        CaseSensitivity = 64,
        Bit = 8192
    };
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
        Exact
    }

    public enum MpDateAfterUnitType {
        None = 0,
        Yesterday,
        LastWeek,
        LastMonth,
        LastYear,
        Exact
    }

    
}