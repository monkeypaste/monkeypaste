﻿namespace CoreOleHandler {
    public enum CoreOleParamType {
        None = 0,
        //readers
        TEXT_R_MAXCHARCOUNT,
        TEXT_R_IGNORE,
        TEXT_W_MAXCHARCOUNT,
        TEXT_W_IGNORE,
        TEXT_W_FILEPRIORITY,
        TEXT_W_FILEEXT,

        TEXTPLAIN_R_MAXCHARCOUNT,
        TEXTPLAIN_R_IGNORE,
        TEXTPLAIN_W_MAXCHARCOUNT,
        TEXTPLAIN_W_IGNORE,
        TEXTPLAIN_W_FILEPRIORITY,
        TEXTPLAIN_W_FILEEXT,

        CSV_R_IGNORE,
        CSV_R_MAXCHARCOUNT,
        CSV_W_IGNORE,
        CSV_W_MAXCHARCOUNT,
        CSV_W_FILEPRIORITY,
        CSV_W_FILEEXT,

        RICHTEXTFORMAT_R_MAXCHARCOUNT,
        RICHTEXTFORMAT_R_IGNORE,
        RICHTEXTFORMAT_R_TOHTML,
        RICHTEXTFORMAT_R_HTMLPARTAGNAME,
        RICHTEXTFORMAT_W_MAXCHARCOUNT,
        RICHTEXTFORMAT_W_IGNORE,
        RICHTEXTFORMAT_W_TOHTML,
        RICHTEXTFORMAT_W_FILEPRIORITY,
        RICHTEXTFORMAT_W_FILEEXT,

        HTMLFORMAT_R_MAXCHARCOUNT,
        HTMLFORMAT_R_IGNORE,
        HTMLFORMAT_R_TORTF,
        HTMLFORMAT_W_MAXCHARCOUNT,
        HTMLFORMAT_W_IGNORE,
        HTMLFORMAT_W_TORTF,
        HTMLFORMAT_W_FILEPRIORITY,
        HTMLFORMAT_W_FILEEXT,

        TEXTHTML_R_MAXCHARCOUNT,
        TEXTHTML_R_IGNORE,
        TEXTHTML_R_TORTF,
        TEXTHTML_W_MAXCHARCOUNT,
        TEXTHTML_W_IGNORE,
        TEXTHTML_W_TORTF,
        TEXTHTML_W_FILEPRIORITY,
        TEXTHTML_W_FILEEXT,

        TEXTXMOZURLPRIV_R_IGNORE,
        TEXTXMOZURLPRIV_W_IGNORE,

        PNG_R_IGNORE,
        PNG_R_MAXW,
        PNG_R_MAXH,
        PNG_R_SCALEOVERSIZED,
        PNG_R_IGNORE_EMPTY,
        PNG_W_IGNORE,
        PNG_W_FROMTEXTFORMATS,
        PNG_W_ASCIIART,
        PNG_W_FILEPRIORITY,
        PNG_W_FILEEXT,

        FILES_R_IGNORE,
        FILES_R_IGNOREEXTS,
        FILES_R_IGNOREDIRS,
        FILES_W_IGNORE,
        FILES_W_IGNOREEXTS,
        
        XSPECIALGNOMECOPIEDFILES_R_IGNORE,
        XSPECIALGNOMECOPIEDFILES_W_IGNORE,

        TEXTURILIST_R_IGNORE,
        TEXTURILIST_W_IGNORE,
    }
}