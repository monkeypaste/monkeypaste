
using MonkeyPaste.Common;
using System;
using System.Collections.Generic;

namespace MonkeyPaste {
    public enum MpContentSortType {
        CopyDateTime,
        Source,
        Title,
        ItemData,
        ItemType,
        UsageScore,
    }

    [Flags]
    public enum MpContentQueryBitFlags : long {
        None = 0,
        CaseSensitive = 1L << 0,
        Title = 1L << 1,
        TextType = 1L << 2,
        FileType = 1L << 3,
        ImageType = 1L << 4,
        Url = 1L << 5,
        AppName = 1L << 6,
        AppPath = 1L << 7,
        Annotations = 1L << 8,
        Tag = 1L << 9,
        Regex = 1L << 10,
        Content = 1L << 11,
        UrlTitle = 1L << 12,
        Time = 1L << 13,
        WholeWord = 1L << 14,
        DeviceType = 1L << 15,
        DeviceName = 1L << 16,

        ItemColor = 1L << 17,

        Matches = 1L << 18,
        Contains = 1L << 19,
        BeginsWith = 1L << 20,
        EndsWith = 1L << 21,

        Width = 1L << 22,
        Height = 1L << 23,

        Hex = 1L << 24,
        Rgba = 1L << 25,
        ColorDistance = 1L << 26,

        Equals = 1L << 27,
        GreaterThan = 1L << 28,
        LessThan = 1L << 29,
        IsNot = 1L << 30,

        Exactly = 1L << 31,
        Before = 1L << 32,
        After = 1L << 33,
        WithinLast = 1L << 34,

        FileName = 1L << 35,
        FilePath = 1L << 36,
        FileExt = 1L << 37,

        UrlDomain = 1L << 38,

        Created = 1L << 39, // mirrors MpTransactionType
        Dropped = 1L << 40,
        Dragged = 1L << 41,
        Pasted = 1L << 42,
        Copied = 1L << 43,
        Cut = 1L << 44,
        Edited = 1L << 45,
        Analyzed = 1L << 46,
        Appended = 1L << 47,
        Recreated = 1L << 48,
        Error = 1L << 49,
        System = 1L << 50,

        Hours = 1L << 51,
        Days = 1L << 52,
        Weeks = 1L << 53,
        Months = 1L << 54,
        Years = 1L << 55,

        And = 1L << 56,
        Or = 1L << 57,
        Not = 1L << 58
    }

    public static class MpQueryEnumExtensions {
        #region State Helpers
        public static bool HasMultiValue(this MpContentQueryBitFlags cqbf) {
            return
                //cqbf.HasFlag(MpContentQueryBitFlags.DateTimeRange) ||
                cqbf.HasFlag(MpContentQueryBitFlags.Rgba) ||
                cqbf.HasFlag(MpContentQueryBitFlags.FileExt);// ||
                                                             //cqbf.HasFlag(MpContentQueryBitFlags.WithinLast);
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
                case MpContentQueryBitFlags.WithinLast:
                case MpContentQueryBitFlags.FileName:
                case MpContentQueryBitFlags.FilePath:
                case MpContentQueryBitFlags.FileExt:
                case MpContentQueryBitFlags.UrlDomain:
                case MpContentQueryBitFlags.Created:
                case MpContentQueryBitFlags.Dropped:
                case MpContentQueryBitFlags.Dragged:
                case MpContentQueryBitFlags.Pasted:
                case MpContentQueryBitFlags.Copied:
                case MpContentQueryBitFlags.Cut:
                case MpContentQueryBitFlags.Edited:
                case MpContentQueryBitFlags.Analyzed:
                case MpContentQueryBitFlags.Appended:
                case MpContentQueryBitFlags.Recreated:
                case MpContentQueryBitFlags.Error:
                case MpContentQueryBitFlags.System:
                    return true;
                default:
                    return false;
            }
        }

        public static bool HasTitleMatchFilterFlag(this MpContentQueryBitFlags qf) {
            return
                qf.HasFlag(MpContentQueryBitFlags.Title);
        }
        public static bool HasContentMatchFilterFlag(this MpContentQueryBitFlags qf) {
            return
                qf.HasFlag(MpContentQueryBitFlags.Content) ||
                qf.HasFlag(MpContentQueryBitFlags.FileName) ||
                qf.HasFlag(MpContentQueryBitFlags.FilePath) ||
                qf.HasFlag(MpContentQueryBitFlags.FileExt);
        }

        public static bool HasSourceMatchFilterFlag(this MpContentQueryBitFlags qf) {
            return
                qf.HasFlag(MpContentQueryBitFlags.Url) ||
                qf.HasFlag(MpContentQueryBitFlags.UrlTitle) ||
                qf.HasFlag(MpContentQueryBitFlags.UrlDomain) ||
                qf.HasFlag(MpContentQueryBitFlags.AppName) ||
                qf.HasFlag(MpContentQueryBitFlags.AppPath) ||
                qf.HasFlag(MpContentQueryBitFlags.Annotations) ||
                qf.HasFlag(MpContentQueryBitFlags.DeviceName);
        }

        public static bool HasStringMatchFilterFlag(this MpContentQueryBitFlags qf) {
            return
                qf.HasFlag(MpContentQueryBitFlags.Title) ||
                qf.HasFlag(MpContentQueryBitFlags.Content) ||
                qf.HasFlag(MpContentQueryBitFlags.Url) ||
                qf.HasFlag(MpContentQueryBitFlags.UrlTitle) ||
                qf.HasFlag(MpContentQueryBitFlags.AppName) ||
                qf.HasFlag(MpContentQueryBitFlags.AppPath) ||
                qf.HasFlag(MpContentQueryBitFlags.Annotations) ||
                qf.HasFlag(MpContentQueryBitFlags.DeviceName) ||
                qf.HasFlag(MpContentQueryBitFlags.FileName) ||
                qf.HasFlag(MpContentQueryBitFlags.FilePath) ||
                qf.HasFlag(MpContentQueryBitFlags.FileExt) ||
                qf.HasFlag(MpContentQueryBitFlags.UrlDomain);
        }
        public static bool IsStringMatchFilterFlag(this MpContentQueryBitFlags f) {
            if (!f.IsViewFieldFlag()) {
                return false;
            }
            return HasStringMatchFilterFlag(f);
        }

        #endregion

        #region Filter Helpers

        public static string GetWithinLastTicks(this MpContentQueryBitFlags qf, string mv) {
            double unit_val;
            try {
                unit_val = double.Parse(mv);
            }
            catch (Exception ex) {
                MpConsole.WriteTraceLine($"Error parsing within last unit value of '{mv}'", ex);
                return string.Empty;
            }

            TimeSpan ts;
            if (qf.HasFlag(MpContentQueryBitFlags.Hours)) {
                ts = TimeSpan.FromHours(unit_val);
            } else if (qf.HasFlag(MpContentQueryBitFlags.Days)) {
                ts = TimeSpan.FromDays(unit_val);
            } else if (qf.HasFlag(MpContentQueryBitFlags.Weeks)) {
                ts = TimeSpan.FromDays(unit_val * 7);
            } else if (qf.HasFlag(MpContentQueryBitFlags.Months)) {
                ts = TimeSpan.FromDays(unit_val * 30);
            } else if (qf.HasFlag(MpContentQueryBitFlags.Years)) {
                ts = TimeSpan.FromDays(unit_val * 365);
            }
            if (ts == default) {
                return string.Empty;
            }

            var now_offset_dt = DateTime.Now - ts;
            return now_offset_dt.Ticks.ToString();
        }
        public static string GetTransactionType(this MpContentQueryBitFlags qf) {
            for (int i = 1; i < typeof(MpTransactionType).Length(); i++) {
                MpContentQueryBitFlags cur_qf = ((MpTransactionType)i).ToString().ToEnum<MpContentQueryBitFlags>();
                if (qf.HasFlag(cur_qf)) {
                    return cur_qf.ToString();
                }
            }
            return string.Empty;
        }
        public static string GetNumericOperator(this MpContentQueryBitFlags qf) {
            if (qf.HasFlag(MpContentQueryBitFlags.LessThan)) {
                return "<";
            }
            if (qf.HasFlag(MpContentQueryBitFlags.GreaterThan)) {
                return ">";
            }
            if (qf.HasFlag(MpContentQueryBitFlags.IsNot)) {
                return "!=";
            }
            if (qf.HasFlag(MpContentQueryBitFlags.Equals)) {
                return "=";
            }
            return string.Empty;
        }
        public static string ToViewFieldName(this MpContentQueryBitFlags f) {
            switch (f) {
                case MpContentQueryBitFlags.FileName:
                    return "FILE_NAME_FILTER(ItemData)";
                case MpContentQueryBitFlags.FileExt:
                    return "FILE_EXT_FILTER(ItemData)";
                case MpContentQueryBitFlags.FilePath:
                case MpContentQueryBitFlags.Content:
                    return "ItemData";
                case MpContentQueryBitFlags.Url:
                    return "UrlPath";
                case MpContentQueryBitFlags.Annotations:
                    return nameof(MpCopyItem.ItemMetaData);
                case MpContentQueryBitFlags.Created:
                case MpContentQueryBitFlags.Dropped:
                case MpContentQueryBitFlags.Dragged:
                case MpContentQueryBitFlags.Pasted:
                case MpContentQueryBitFlags.Copied:
                case MpContentQueryBitFlags.Cut:
                case MpContentQueryBitFlags.Edited:
                case MpContentQueryBitFlags.Analyzed:
                case MpContentQueryBitFlags.Appended:
                case MpContentQueryBitFlags.Recreated:
                case MpContentQueryBitFlags.Error:
                case MpContentQueryBitFlags.System:
                    return "TransactionLabel";
                case MpContentQueryBitFlags.TextType:
                case MpContentQueryBitFlags.ImageType:
                case MpContentQueryBitFlags.FileType:
                    return "e_MpCopyItemType";
                case MpContentQueryBitFlags.DeviceName:
                    return "DeviceGuid";
                default:
                    return f.ToString();
            }
        }
        public static IEnumerable<string> GetStringMatchFieldNames(this MpContentQueryBitFlags f) {
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

        public static MpContentQueryBitFlags GetStringMatchType(this MpContentQueryBitFlags qf) {
            // NOTE should only contain 1 of the string match types
            if (qf.HasFlag(MpContentQueryBitFlags.Matches)) {
                return MpContentQueryBitFlags.Matches;
            }
            if (qf.HasFlag(MpContentQueryBitFlags.Contains)) {
                return MpContentQueryBitFlags.Contains;
            }
            if (qf.HasFlag(MpContentQueryBitFlags.BeginsWith)) {
                return MpContentQueryBitFlags.BeginsWith;
            }
            if (qf.HasFlag(MpContentQueryBitFlags.EndsWith)) {
                return MpContentQueryBitFlags.EndsWith;
            }
            if (qf.HasFlag(MpContentQueryBitFlags.Regex)) {
                return MpContentQueryBitFlags.Regex;
            }
            return MpContentQueryBitFlags.None;
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
            if (f.HasFlag(MpContentQueryBitFlags.Matches)) {
                return matchVal;
            }
            if (f.HasFlag(MpContentQueryBitFlags.BeginsWith)) {
                return $"{matchVal}{op_symbol}";
            }
            if (f.HasFlag(MpContentQueryBitFlags.EndsWith)) {
                return $"{op_symbol}{matchVal}";
            }
            // contains or no str match flag

            return $"{op_symbol}{matchVal}{op_symbol}";
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