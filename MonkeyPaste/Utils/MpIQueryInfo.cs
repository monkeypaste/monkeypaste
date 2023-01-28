using MonkeyPaste.Avalonia;
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
    public enum MpContentQueryBitFlags {
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
        MatchValue = 131072,
        DateTime = 262_144,
        DateTimeRange = 524_288
    }

    // Criteria Item Flags
    public enum MpDateTimeQueryType {
        None = 0,
        Exactly,
        Before,
        After,
        Between
    }

    public enum MpLogicalQueryType {
        None = 0,
        And,
        Or,
        Not
    }



    public enum MpTextQueryType {
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


    public interface MpIQueryInfo : MpIDbIdCollection, MpIJsonObject {
        public int TotalAvailableItemsInQuery { get; }

        public bool IsDescending { get; }

        public MpContentSortType SortType { get; }

        public MpContentQueryBitFlags FilterFlags { get; }

        public MpLogicalQueryType PrevJoinType { get; } // advanced

        public MpTextQueryType TextFlags { get; } // advanced

        public MpDateTimeQueryType TimeFlags { get;  } // advanced
        MpIQueryInfo Next { get; } // advanced

        public int TagId { get;}

        public string SearchText { get;}

        public int SortOrderIdx { get; }
        

        void NotifyQueryChanged(bool forceRequery = false);

        void RestoreProviderValues();

        void RegisterProvider(MpIQueryInfoValueProvider provider);

        IEnumerable<MpIQueryInfoValueProvider> Providers { get; }

        Task<List<MpCopyItem>> FetchIdsByQueryIdxListAsync(List<int> copyItemQueryIdxList);
        Task QueryForTotalCountAsync();
    }
}
