using MonkeyPaste.Avalonia;
using MonkeyPaste.Common;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MonkeyPaste {

    public enum MpQueryType {
        None = 0,
        Simple,
        Advanced
    }
    //public interface MpIQueryInfoValueProvider {
    //    object Source { get; }
    //    string SourcePropertyName { get; }
    //    string QueryValueName { get; }
    //}

    public interface MpIQueryResultProvider : MpIJsonObject {
        bool CanRequery { get; }
        public int TotalAvailableItemsInQuery { get; }
        Task<List<MpCopyItem>> FetchItemsByQueryIdxListAsync(IEnumerable<int> copyItemQueryIdxList, IEnumerable<int> idsToOmit);
        Task QueryForTotalCountAsync();

        void NotifyQueryChanged(bool forceRequery = false);

        void RestoreProviderValues();

        MpIDbIdCollection PageTools { get; }
    }

    public interface MpIQueryInfo {
        MpQueryType QueryType { get; }
        //public MpTextQueryType TextFlags { get; } // advanced

        //public MpDateTimeQueryType TimeFlags { get; } // advanced
        public MpLogicalQueryType JoinType { get; } // advanced
        MpIQueryInfo Next { get; } // advanced

        public bool IsDescending { get; }

        public MpContentSortType SortType { get; }

        public MpContentQueryBitFlags QueryFlags { get; }


        public int TagId { get; }

        public string MatchValue { get; }

        public int SortOrderIdx { get; }


    }
}
