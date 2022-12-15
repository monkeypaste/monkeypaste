using MonkeyPaste.Common;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace MonkeyPaste {
    public enum MpContentSortType {
        CopyDateTime,
        Source,
        Title,
        ItemData,
        ItemType,
        UsageScore
    }

    [Flags]
    public enum MpContentFilterType {
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
        DeviceName = 65536
    }

    // Criteria Item Flags
    [Flags]
    public enum MpTimeFilterFlagType {
        None = 0,
        Exactly,
        Before,
        After
    }

    [Flags]
    public enum MpLogicalFilterFlagType {
        None = 0,
        And,
        Or
    }

    [Flags]
    public enum MpTextFilterFlagType {
        None = 0,
        Matches,
        Contains,
        BeginsWith,
        EndsWith,
        RegEx
    }

    public interface MpIQueryInfoValueProvider {
        object Source { get; }
        string SourcePropertyName { get; }
        string QueryValueName { get; }
    }

    public interface MpIQueryInfo : MpIJsonObject {
        public int TotalAvailableItemsInQuery { get; }

        public bool IsDescending { get;  set; }

        public MpContentSortType SortType { get; set; }

        public MpContentFilterType FilterFlags { get; set; }

        public MpLogicalFilterFlagType NextJoinType { get; set; }

        //public MpTextFilterFlagType TextFlags { get; set; }

        public MpTimeFilterFlagType TimeFlags { get; set; }

        public int TagId { get; set; }

        public string SearchText { get;  set; }

        public int SortOrderIdx { get; set; }
        MpIQueryInfo Next { get; }

        void NotifyQueryChanged(bool forceRequery = false);
    }
}
