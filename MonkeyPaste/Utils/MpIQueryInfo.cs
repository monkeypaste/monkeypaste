using MonkeyPaste.Avalonia;
using MonkeyPaste.Common;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace MonkeyPaste {
    

    public interface MpIQueryInfoValueProvider {
        object Source { get; }
        string SourcePropertyName { get; }
        string QueryValueName { get; }
    }

    public interface MpIQueryResultProvider : MpIJsonObject {
        
        public int TotalAvailableItemsInQuery { get; }
        Task<List<MpCopyItem>> FetchIdsByQueryIdxListAsync(List<int> copyItemQueryIdxList);
        Task QueryForTotalCountAsync();

        void NotifyQueryChanged(bool forceRequery = false);

        void RestoreProviderValues();

        void RegisterProvider(MpIQueryInfoValueProvider provider);

        IEnumerable<MpIQueryInfoValueProvider> ValueProviders { get; }

        MpIDbIdCollection PageTools { get;}
    }

    public interface MpIQueryInfo {

        //public MpTextQueryType TextFlags { get; } // advanced

        //public MpDateTimeQueryType TimeFlags { get; } // advanced
        public MpLogicalQueryType NextJoinType { get; } // advanced
        MpIQueryInfo Next { get; } // advanced

        public bool IsDescending { get; }

        public MpContentSortType SortType { get; }

        public MpContentQueryBitFlags QueryFlags { get; } 


        public int TagId { get;}

        public string MatchValue { get;}

        public int SortOrderIdx { get; }
        

    }
}
