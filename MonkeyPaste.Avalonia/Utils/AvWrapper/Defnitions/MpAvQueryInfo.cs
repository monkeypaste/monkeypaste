using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MonkeyPaste;
using MonkeyPaste.Common;
using Newtonsoft.Json;

namespace MonkeyPaste.Avalonia {
    public class MpAvQueryInfo : MpIQueryInfo {
        
        public int TotalItemsInQuery { get; set; } = 0;
        public bool IsDescending { get; set; } = true;
        public MpContentSortType SortType { get; set; } = MpContentSortType.CopyDateTime;
        public int TagId { get; set; } = 0;
        public string SearchText { get; set; } = string.Empty;

        public MpContentFilterType FilterFlags { get; set; } = MpContentFilterType.None;
        public MpTextFilterFlagType TextFlags { get; set; } = MpTextFilterFlagType.None;
        public MpTimeFilterFlagType TimeFlags { get; set; } = MpTimeFilterFlagType.None;
        public MpLogicalFilterFlagType LogicFlags { get; set; }

        public int PageSize { get; set; } = 0;

        public int SortOrderIdx { get; set; } = 0;

        public string Serialize() {
            return MpJsonObject.SerializeObject(this);
        }
        
        public void NotifyQueryChanged(bool isFilterSortOrSearch = true) {
            IsDescending = true;// MpClipTileSortViewModel.Instance.IsSortDescending;
            SortType = MpContentSortType.CopyDateTime; // MpClipTileSortViewModel.Instance.SelectedSortType.SortType;
            TagId = MpTag.AllTagId; //MpAvTagTrayViewModel.Instance.SelectedTagTile.TagId;
            SearchText = string.Empty; // MpSearchBoxViewModel.Instance.SearchText;
            TotalItemsInQuery = MpDataModelProvider.TotalTilesInQuery;

            // NOTE not sure why this isn't set so maybe bad
            FilterFlags = MpContentFilterType.TextType | MpContentFilterType.FileType | MpContentFilterType.ImageType; //MpSearchBoxViewModel.Instance.FilterType;

            MpPrefViewModel.Instance.LastQueryInfoJson = Serialize();

            var qi = MpDataModelProvider.QueryInfo;
            MpDataModelProvider.QueryInfos.Clear();

            qi.FilterFlags = FilterFlags;//MpSearchBoxViewModel.Instance.FilterType;
            MpDataModelProvider.QueryInfos.Add(qi);
            // MpSearchBoxViewModel.Instance.CriteriaItems.OrderBy(x => x.SortOrderIdx).ForEach(x => MpDataModelProvider.QueryInfos.Add(x.ToQueryInfo()));

            MpPlatformWrapper.Services.MainThreadMarshal.RunOnMainThread(() => {
                if (isFilterSortOrSearch) {
                    MpMessenger.SendGlobal<MpMessageType>(MpMessageType.QueryChanged);
                } else {
                    MpMessenger.SendGlobal<MpMessageType>(MpMessageType.SubQueryChanged);
                }
            });
        }

    }
}
