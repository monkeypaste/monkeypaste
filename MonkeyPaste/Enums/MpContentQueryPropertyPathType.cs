namespace MonkeyPaste {
    public enum MpContentQueryPropertyPathType {
        None = 0,
        ItemData,
        ItemType,
        Title, //seperator
        AppPath,
        AppName,
        UrlPath,
        UrlTitle,
        UrlDomainPath, //seperator
        CopyDateTime,
        LastPasteDateTime, //seperator
        CopyCount,
        PasteCount,
        SourceDeviceName,
        SourceDeviceType,
        LastOutput,
    }

    public enum MpContentQueryPropertyGroupType {
        Root = 0,
        App,
        Device,
        Meta,
        Statistics,
        Url,
    }

}
