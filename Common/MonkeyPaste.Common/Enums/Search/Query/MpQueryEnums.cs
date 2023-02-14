
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace MonkeyPaste.Common {
    public enum MpContentSortType {
        CopyDateTime,
        Source,
        Title,
        ItemData,
        ItemType,
        UsageScore
    }

    [Flags]
    public enum MpContentQueryBitFlags : Int64 {
        None = 0,
        CaseSensitive = 1,
        Title = 2,
        TextType = 4,
        FileType = 8,
        ImageType = 16,
        Url = 32,
        AppName = 64,
        AppPath = 128,
        Annotations = 256,
        Tag = 512,
        Regex = 1024,
        Content = 2048,
        UrlTitle = 4096,
        Time = 8192,
        WholeWord = 16384,
        DeviceType = 32768,
        DeviceName = 65536,
        //MatchValue = 131072,
        //DateTime = 262_144,
        //DateTimeRange = 524_288,

        Matches = 1_048_576,
        Contains = 2_097_152,
        BeginsWith = 4_194_304,
        EndsWith = 8_388_608,

        Width = 16_777_216,
        Height = 33_554_432,

        Hex = 67_108_864,
        Rgba = 134_217_728,

        Exactly = 268_435_456,
        Before = 536_870_912,
        After = 1_073_741_824,
        Between = 2_147_483_648,

        FileName = 4_294_967_296,
        FilePath = 8_589_934_592,
        FileExt = 17_179_869_184,

        UrlDomain = 34_359_738_368,

        Created = 68_719_476_736,
        Modified = 137_438_953_472,
        Pasted = 274_877_906_944,

        Hours = 549_755_813_888,
        Days = 1_099_511_627_776,
        //Weeks = 2_199_023_255_552,
        //Months = 4_398_046_511_104,
        //Years = 8_796_093_022_208, //45 8796093022208

        And = 17_592_186_044_416,
        Or = 35_184_372_088_832,
        Not = 70_368_744_177_664
    }

    public static class MpQueryEnumExtensions {
        #region State Helpers
        public static bool HasMultiValue(this MpContentQueryBitFlags cqbf) {
            return
                //cqbf.HasFlag(MpContentQueryBitFlags.DateTimeRange) ||
                cqbf.HasFlag(MpContentQueryBitFlags.Rgba) ||
                cqbf.HasFlag(MpContentQueryBitFlags.FileExt) ||
                cqbf.HasFlag(MpContentQueryBitFlags.Between);
        }

        public static bool HasTimeSpanValue(this MpContentQueryBitFlags cqbf) {
            return
                cqbf.HasFlag(MpContentQueryBitFlags.Hours) ||
                cqbf.HasFlag(MpContentQueryBitFlags.Days) ||
                cqbf.HasFlag(MpContentQueryBitFlags.Exactly);
        }


        public static bool IsViewFieldFlag(this MpContentQueryBitFlags cqbf) {
            switch (cqbf) {
                case MpContentQueryBitFlags.Title:
                case MpContentQueryBitFlags.Content:
                case MpContentQueryBitFlags.Url:
                case MpContentQueryBitFlags.UrlTitle:
                case MpContentQueryBitFlags.AppName:
                case MpContentQueryBitFlags.AppPath:
                case MpContentQueryBitFlags.Annotations:
                case MpContentQueryBitFlags.DeviceName:
                case MpContentQueryBitFlags.DeviceType:
                case MpContentQueryBitFlags.Width:
                case MpContentQueryBitFlags.Height:
                case MpContentQueryBitFlags.Hex:
                case MpContentQueryBitFlags.Rgba:
                case MpContentQueryBitFlags.Exactly:
                case MpContentQueryBitFlags.Before:
                case MpContentQueryBitFlags.After:
                case MpContentQueryBitFlags.Between:
                case MpContentQueryBitFlags.FileName:
                case MpContentQueryBitFlags.FilePath:
                case MpContentQueryBitFlags.FileExt:
                case MpContentQueryBitFlags.UrlDomain:
                case MpContentQueryBitFlags.Created:
                case MpContentQueryBitFlags.Modified:
                case MpContentQueryBitFlags.Pasted:
                    return true;
                default:
                    return false;
            }
        }

        public static bool IsStringMatchFilterFlag(this MpContentQueryBitFlags f) {
            if (!f.IsViewFieldFlag()) {
                return false;
            }
            switch (f) {
                case MpContentQueryBitFlags.Title:
                case MpContentQueryBitFlags.Content:
                case MpContentQueryBitFlags.Url:
                case MpContentQueryBitFlags.UrlTitle:
                case MpContentQueryBitFlags.AppName:
                case MpContentQueryBitFlags.AppPath:
                case MpContentQueryBitFlags.Annotations:
                case MpContentQueryBitFlags.DeviceName:
                case MpContentQueryBitFlags.FileName:
                case MpContentQueryBitFlags.FilePath:
                case MpContentQueryBitFlags.FileExt:
                case MpContentQueryBitFlags.UrlDomain:
                    return true;
                default:
                    return false;
            }
        }

        #endregion

        #region Filter Helpers

        public static string ToViewFieldName(this MpContentQueryBitFlags f) {
            switch (f) {
                case MpContentQueryBitFlags.Content:
                    return "ItemData";
                case MpContentQueryBitFlags.Url:
                    return "UrlPath";
                case MpContentQueryBitFlags.Annotations:
                    return "ItemMetaData";
                case MpContentQueryBitFlags.Created:
                    return "CopyDateTime";
                case MpContentQueryBitFlags.Modified:
                case MpContentQueryBitFlags.Pasted:
                    return "TransactionDateTime";
                case MpContentQueryBitFlags.TextType:
                case MpContentQueryBitFlags.ImageType:
                case MpContentQueryBitFlags.FileType:
                    return "e_MpCopyItemType";
                default:
                    return f.ToString();
            }
        }
        public static IEnumerable<string> GetStringMatchFieldName(this MpContentQueryBitFlags f) {
            foreach (string flag_name in typeof(MpContentQueryBitFlags).GetEnumNames()) {
                MpContentQueryBitFlags cur_flag = flag_name.ToEnum<MpContentQueryBitFlags>();
                if (!cur_flag.IsStringMatchFilterFlag()) {
                    continue;
                }
                if (f.HasFlag(cur_flag)) {
                    yield return cur_flag.ToViewFieldName();
                }
            }
        }

        public static string GetDateTimeTickValue(this MpContentQueryBitFlags f, string mv) {
            double today_offset = (DateTime.Now - DateTime.Today).TotalDays;
            double days = double.Parse(mv);
            double total_day_offset = days + today_offset;
            var dt = DateTime.Now - TimeSpan.FromDays(total_day_offset);
            string match_ticks = dt.Ticks.ToString();
            return match_ticks;
        }

        public static string GetStringMatchOp(this MpContentQueryBitFlags f) {

            if (f.HasFlag(MpContentQueryBitFlags.Regex)) {
                // if regex is set, case sensitive and whole word will be disabled
                return "REGEXP";
            }

            if (f.HasFlag(MpContentQueryBitFlags.WholeWord)) {
                // regardless of case sensitive whole word is regex
                // and distinguished as not 'actual' regex by the wholeword flag
                return "REGEXP";
            }
            if (f.HasFlag(MpContentQueryBitFlags.CaseSensitive)) {
                return "GLOB";
            }
            return "LIKE";
        }

        public static string GetStringMatchValue(this MpContentQueryBitFlags f, string matchOp, string matchVal) {
            if (matchOp == "REGEXP") {
                if (f.HasFlag(MpContentQueryBitFlags.WholeWord)) {
                    //string flags = "m";
                    //if (!f.HasFlag(MpContentQueryBitFlags.CaseSensitive)) {
                    //    flags += "i";
                    //}
                    //return $"(?{flags})\b{matchVal}\b";
                    if (!f.HasFlag(MpContentQueryBitFlags.CaseSensitive)) {
                        matchVal = matchVal.ToUpper();
                    }
                    return $@"\b{matchVal}\b";
                }
                return $"{matchVal}";
            }
            string op_symbol = matchOp == "GLOB" ? "*" : "%";
            switch (f) {
                case MpContentQueryBitFlags.Matches:
                    return matchVal;
                case MpContentQueryBitFlags.BeginsWith:
                    return $"{matchVal}{op_symbol}";
                case MpContentQueryBitFlags.EndsWith:
                    return $"{op_symbol}{matchVal}";
                case MpContentQueryBitFlags.Contains:
                default:
                    return $"{op_symbol}{matchVal}{op_symbol}";
            }
        }


        public static DateTime? ToDateTime(this MpContentQueryBitFlags cqbf, string mv) {
            if (!cqbf.HasTimeSpanValue()) {
                return null;
            }
            if (cqbf.HasFlag(MpContentQueryBitFlags.Exactly)) {
                try {
                    var dt = DateTime.Parse(mv);
                    if (cqbf.HasFlag(MpContentQueryBitFlags.Before)) {

                    }
                }
                catch {
                    return null;
                }
            }
            double v = 0;
            try {
                v = double.Parse(mv);
            }
            catch {
                return null;
            }
            if (v == 0) {
                return null;
            }
            if (v < 0) {
                // is this ok?
                Debugger.Break();
            }
            if (cqbf.HasFlag(MpContentQueryBitFlags.Hours)) {
                //return TimeSpan.FromHours(v);
            }
            // all timespans are factored into day values
            var ts = TimeSpan.FromDays(v);
            if (cqbf.HasFlag(MpContentQueryBitFlags.Before)) {
                return DateTime.Now - ts;
            }
            if (cqbf.HasFlag(MpContentQueryBitFlags.After)) {
                return DateTime.Now - ts;
            }
            return null;
        }

        public static string GetMatchValueModelToken(this MpContentQueryBitFlags sqf, string st) {
            st = st == null ? string.Empty : st;
            return $"{sqf.HasFlag(MpContentQueryBitFlags.CaseSensitive)},{sqf.HasFlag(MpContentQueryBitFlags.WholeWord)},{sqf.HasFlag(MpContentQueryBitFlags.Regex)},{st.ToBase64String()}";
        }

        #endregion
    }

    // Criteria Item Flags
    //public enum MpDateTimeQueryType {
    //    None = 0,
    //    Exactly,
    //    Before,
    //    After,
    //    Between
    //}

    public enum MpLogicalQueryType {
        None = 0,
        And,
        Or,
        Not
    }

    //public enum MpTextQueryType {
    //    None = 0,
    //    Matches,
    //    Contains,
    //    BeginsWith,
    //    EndsWith,
    //    RegEx
    //}

}