using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MonkeyPaste;

namespace MpWpfApp {
    public class MpWpfQueryInfo : MpIQueryInfo {
        
        public int TotalItemsInQuery { get; set; } = 0;
        public bool IsDescending { get; set; } = true;
        public MpContentSortType SortType { get; set; } = MpContentSortType.None;
        public int TagId { get; set; } = 0;
        public string SearchText { get; set; } = string.Empty;

        public MpContentFilterType FilterFlags { get; set; } = MpContentFilterType.None;
        public MpTextFilterFlagType TextFlags { get; set; } = MpTextFilterFlagType.None;
        public MpTimeFilterFlagType TimeFlags { get; set; } = MpTimeFilterFlagType.None;
        public MpLogicalFilterFlagType LogicFlags { get; set; }

        public int PageSize { get; set; } = 0;

        public int SortOrderIdx { get; set; } = 0;

        
        public void NotifyQueryChanged(bool isFilterSortOrSearch = true) {
            IsDescending = MpClipTileSortViewModel.Instance.IsSortDescending;
            SortType = MpClipTileSortViewModel.Instance.SelectedSortType.SortType;
            TagId = MpTagTrayViewModel.Instance.SelectedTagTile.TagId;
            SearchText = MpSearchBoxViewModel.Instance.SearchText;
            TotalItemsInQuery = MpDataModelProvider.TotalTilesInQuery;

            var qi = MpDataModelProvider.QueryInfo;
            MpDataModelProvider.QueryInfos.Clear();

            qi.FilterFlags = MpSearchBoxViewModel.Instance.FilterType;
            MpDataModelProvider.QueryInfos.Add(qi);
            // MpSearchBoxViewModel.Instance.CriteriaItems.OrderBy(x => x.SortOrderIdx).ForEach(x => MpDataModelProvider.QueryInfos.Add(x.ToQueryInfo()));

            MpHelpers.RunOnMainThread(() => {
                if (isFilterSortOrSearch) {
                    MpMessenger.Send<MpMessageType>(MpMessageType.QueryChanged);
                } else {
                    MpMessenger.Send<MpMessageType>(MpMessageType.SubQueryChanged);
                }
            });
        }

    }
}
