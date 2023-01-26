using System;

namespace MonkeyPaste {
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
}
