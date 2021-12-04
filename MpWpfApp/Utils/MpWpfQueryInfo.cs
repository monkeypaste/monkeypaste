using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MonkeyPaste;

namespace MpWpfApp {
    public class MpWpfQueryInfo : MpIQueryInfo {
        public int TotalItemsInQuery { get; set; }
        public bool IsDescending { get; set; }
        public MpContentSortType SortType { get; set; }
        public int TagId { get; set; }
        public string SearchText { get; set; }
        public MpContentFilterType FilterFlags { get; set; }
        public int PageSize { get; set; }

        public void NotifyQueryChanged(bool isFilterSortOrSearch = true) {
            IsDescending = MpClipTileSortViewModel.Instance.IsSortDescending;
            SortType = MpClipTileSortViewModel.Instance.SelectedSortType.SortType;
            TagId = MpTagTrayViewModel.Instance.SelectedTagTile.TagId;
            SearchText = MpSearchBoxViewModel.Instance.SearchText;
            FilterFlags = MpSearchBoxViewModel.Instance.FilterType;
            TotalItemsInQuery = MpDataModelProvider.Instance.TotalItems;

            if(isFilterSortOrSearch) {
                MpMessenger.Instance.Send<MpMessageType>(MpMessageType.QueryChanged);
            } else {
                MpMessenger.Instance.Send<MpMessageType>(MpMessageType.SubQueryChanged);
            }
        }

        
    }
}
