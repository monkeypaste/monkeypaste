﻿namespace MonkeyPaste {
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
        ItemRefUrl,
        MAX_COPY_ITEM_TYPE,
        LastOutput,
        AnalyzerLabel
    }

}