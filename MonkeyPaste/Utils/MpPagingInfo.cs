using System;
using System.Collections.Generic;
using System.Text;

namespace MonkeyPaste {
    public class MpPagingInfo {
        public int Offset { get; set; } = 0;
        public int Count { get; set; } = 0;
    }

    public class MpSortInfo {
        public bool IsDescending { get; set; } = true;
        public MpClipTileSortType SortType { get; set; } = MpClipTileSortType.CopyDateTime;
    }

    public class MpFilterInfo {
        public int TagId { get; set; } = MpTag.RecentTagId;

        public string SearchText { get; set; } = string.Empty;


    }
}
