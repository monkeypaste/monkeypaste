
namespace MonkeyPaste.Common {
    public enum MpRootOptionType {
        None = 0,
        Content,
        ContentType,
        Collection,
        Source,
        DateOrTime
    }

    public enum MpContentOptionType {
        None = 0,
        AnyText,
        TypeSpecific,
        Title
    }

    public enum MpContentTypeOptionType {
        None = 0,
        Text,
        Image,
        Files
    }

    public enum MpSourceOptionType {
        None = 0,
        Device,
        App,
        Website
    }

    public enum MpAppOptionType {
        None = 0,
        ApplicationName,
        ProcessPath
    }

    public enum MpWebsiteOptionType {
        None = 0,
        Url,
        Domain,
        Title
    }

    public enum MpDateTimeTypeOptionType {
        None = 0,
        Created,
        Modified,
        Pasted
    }

    public enum MpDateTimeOptionType {
        None = 0,
        WithinLast,
        Before,
        After,
        Exact
    }

    public enum MpFileContentOptionType {
        None = 0,
        Path,
        Name,
        Kind
    }

    public enum MpFileOptionType {
        None = 0,
        Document,
        Image,
        Video,
        Spreadsheet,
        Custom
    }

    public enum MpTextOptionType {
        None = 0,
        Matches,
        Contains,
        BeginsWith,
        EndsWith,
        RegEx
    }

    public enum MpImageOptionType {
        None = 0,
        Dimensions,
        Format,
        Description,
        Color
    }

    public enum MpNumberOptionType {
        None = 0,
        Equals,
        GreaterThan,
        LessThan,
        IsNot
    }

    public enum MpDimensionOptionType {
        None = 0,
        Width,
        Height
    }

    public enum MpColorOptionType {
        None = 0,
        Hex,
        RGBA
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