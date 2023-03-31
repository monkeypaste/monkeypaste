using MonkeyPaste.Common;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MonkeyPaste {

    public enum MpQueryType {
        None = 0,
        Simple,
        Advanced
    }

    public interface MpIQueryResultProvider : MpIJsonObject {
        bool IsQuerying { get; }
        bool CanRequery { get; }
        public int TotalAvailableItemsInQuery { get; }
        //Task<List<MpCopyItem>> FetchItemsByQueryIdxListAsync(IEnumerable<int> copyItemQueryIdxList, IEnumerable<int> idsToOmit);
        Task<List<MpCopyItem>> FetchPageAsync(int offset, int limit);
        Task QueryForTotalCountAsync();

        void NotifyQueryChanged(bool forceRequery = false);

        void RestoreProviderValues();

        IEnumerable<MpIQueryInfo> Infos { get; }

        MpIQueryPageTools PageTools { get; }
    }

    public interface MpIQueryInfo {
        MpQueryType QueryType { get; }
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
