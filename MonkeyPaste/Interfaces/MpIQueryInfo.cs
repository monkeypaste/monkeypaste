using MonkeyPaste.Common;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MonkeyPaste {
    // this test branch change
    public enum MpQueryType {
        None = 0,
        Simple,
        Advanced
    }

    public interface MpIQueryResultProvider : MpIJsonObject {
        bool IsQuerying { get; }
        bool CanRequery { get; }
        public int TotalAvailableItemsInQuery { get; }
        Task<List<int>> FetchPageIdsAsync(int offset, int limit);
        Task QueryForTotalCountAsync(bool isRequery);

        Task<int> FetchItemOffsetIdxAsync(int ciid);
        Task<List<MpCopyItem>> QueryForModelsAsync();
        Task<List<MpCopyItem>> FetchPageAsync(int offset, int limit);

        void NotifyQueryChanged(bool forceRequery = false);

        void RestoreProviderValues();

        IEnumerable<MpIQueryInfo> Infos { get; }

        MpIQueryPageTools PageTools { get; }
    }

    public interface MpITextMatchInfo {
        string MatchValue { get; }
        bool CaseSensitive { get; }
        bool WholeWord { get; }
        bool UseRegex { get; }

    }
    public interface MpIQueryInfo : MpITextMatchInfo {
        MpQueryType QueryType { get; }
        MpLogicalQueryType JoinType { get; } // advanced
        MpIQueryInfo Next { get; } // advanced

        bool IsDescending { get; }

        MpContentSortType SortType { get; }

        MpContentQueryBitFlags QueryFlags { get; }


        int TagId { get; }

        int SortOrderIdx { get; }


    }
}
