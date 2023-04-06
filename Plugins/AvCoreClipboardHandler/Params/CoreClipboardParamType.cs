namespace AvCoreClipboardHandler {
    public enum CoreClipboardParamType {
        //readers
        R_MaxCharCount_Text = 1,
        R_Ignore_Text,

        R_MaxCharCount_WebText,
        R_Ignore_WebText,

        R_MaxCharCount_Rtf,
        R_Ignore_Rtf,

        R_MaxCharCount_Html,
        R_Ignore_Html,

        R_MaxCharCount_WebHtml,
        R_Ignore_WebHtml,

        R_Ignore_WebUrl_Linux,

        R_Ignore_Image,

        R_IgnoreAll_FileDrop,
        R_IgnoredExt_FileDrop,
        R_IgnoredDirs_FileDrop,

        R_Ignore_Csv, //16

        //writers
        W_MaxCharCount_Text,
        W_Ignore_Text,

        W_MaxCharCount_WebText,
        W_Ignore_WebText,

        W_MaxCharCount_Rtf,
        W_Ignore_Rtf,

        W_MaxCharCount_Html,
        W_Ignore_Html,

        W_MaxCharCount_WebHtml,
        W_Ignore_WebHtml,

        W_Ignore_WebUrl_Linux, // don't think is used...

        W_Format_Image,
        W_Ignore_Image,

        W_IgnoreAll_FileDrop,
        W_IgnoredExt_FileDrop,

        W_IgnoreAll_FileDrop_Linux,
        W_IgnoreExt_FileDrop_Linux,

        W_Ignore_Csv, // 34

        W_Ignore_Url, // 35
        R_Ignore_Url, // 36

        W_UriList, // 37
        R_UriList, // 38

        W_Ignore_Bitmap, //39
    }
}