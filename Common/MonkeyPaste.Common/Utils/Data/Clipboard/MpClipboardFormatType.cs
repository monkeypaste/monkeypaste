namespace MonkeyPaste.Common {
    public enum MpClipboardFormatType {
        None = 0,
        Text,
        Html,
        Rtf,
        Bitmap,
        FileDrop,
        Csv,
        InternalContent,
        UnicodeText,
        OemText,
        Xaml,
        XamlPackage,
        Custom //when format name doesn't resolve to any previous
    }
}
