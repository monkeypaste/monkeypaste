﻿
using System;
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
        Meta = 256,
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
        public static bool IsMultiValue(this MpContentQueryBitFlags cqbf) {
            return
                //cqbf.HasFlag(MpContentQueryBitFlags.DateTimeRange) ||
                cqbf.HasFlag(MpContentQueryBitFlags.Rgba) ||
                cqbf.HasFlag(MpContentQueryBitFlags.FileExt) ||
                cqbf.HasFlag(MpContentQueryBitFlags.Between);
        }

        public static bool IsTimeSpanValue(this MpContentQueryBitFlags cqbf) {
            return
                cqbf.HasFlag(MpContentQueryBitFlags.Hours) ||
                cqbf.HasFlag(MpContentQueryBitFlags.Days) ||
                cqbf.HasFlag(MpContentQueryBitFlags.Exactly) ;
        }

        public static DateTime? ToDateTime(this MpContentQueryBitFlags cqbf, string mv) {
            if(!cqbf.IsTimeSpanValue()) {
                return null;
            }
            if(cqbf.HasFlag(MpContentQueryBitFlags.Exactly)) {
                try {
                    var dt = DateTime.Parse(mv);
                    if(cqbf.HasFlag(MpContentQueryBitFlags.Before)) {

                    }
                }
                catch {
                    return null;
                }
            }
            double v = 0;
            try {
                v = double.Parse(mv);
            } catch {
                return null;
            }
            if(v == 0) {
                return null;
            }
            if(v < 0) {
                // is this ok?
                Debugger.Break();
            }
            if (cqbf.HasFlag(MpContentQueryBitFlags.Hours)) {
                //return TimeSpan.FromHours(v);
            }
            // all timespans are factored into day values
            var ts = TimeSpan.FromDays(v);
            if(cqbf.HasFlag(MpContentQueryBitFlags.Before)) {
                return DateTime.Now - ts;
            }
            if(cqbf.HasFlag(MpContentQueryBitFlags.After)) {
                return DateTime.Now - ts;
            }
            return null;
        }
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