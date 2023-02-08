
using System;
using System.Collections;
using System.Collections.Generic;

namespace MonkeyPaste.Common {
    public enum MpNextJoinOptionType {
        // NOTE these need to align w/ MpLogicalQueryType
        None = 0,
        All,
        Any,
        NotOne
    }
    public enum MpRootOptionType {
        None = 0,
        Content,
        ContentType,
        //Collection,
        Source,
        DateOrTime
    }

    public enum MpContentOptionType {
        None = 0,
        AnyText,
        TypeSpecific,
        Title,
        Annotation
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
        Audio, //
        Compressed,
        DiscAndMedia,
        DataAndDatabase,
        Document, //
        Email,
        Executable,
        Font,
        Image, //
        Internet,
        Presentation,
        Programming,
        Spreadsheet, //
        System,
        Video, //
        UserDefined,
        Custom //
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

    public static class MpSearchCriteriaOptionExtensions {
        public static string ToOptionPathString(this IList<object> opts, MpContentQueryBitFlags current_flag) {
            List<string> parts = new List<string>();
            foreach (var opt in opts) {
                string opt_str = string.Empty;
                //if(opt is Type optType) {
                //    if(Enum.GetUnderlyingType(optType) is Type typeEnum) {
                //        opt = Enum.Parse(typeEnum, "None");
                //    }
                //}
                if (opt is Enum enumOpt) {
                    opt_str = $"{enumOpt.GetType()}|{enumOpt}";
                } else if (opt is object[] matchParts) {
                    // special case for case sensitive
                    if (matchParts[0] is Enum matchEnum) {
                        opt_str += $"{matchEnum.GetType()}|{matchEnum}";
                    }
                    if (matchParts[1] is string match_text) {
                        opt_str += $"|{match_text.ToBase64String()}";
                    }
                    if (matchParts[2] is bool case_val) {
                        opt_str += $"|{case_val}";
                    }
                }
                opt_str += $"|{current_flag}";

                parts.Add(opt_str);
            }
            string result = string.Join(",", parts);
            return result;
        }
    }
}