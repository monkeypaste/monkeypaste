using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace MonkeyPaste {
    public enum MpContentSortType {
        None = 0,
        CopyDateTime,
        Source,
        Title,
        ItemData,
        ItemType,
        UsageScore,
        Manual
    }

    [Flags]
    public enum MpContentFilterType {
        None = 0,
        CaseSensitive = 1,
        Title = 2,
        Text = 4,
        File = 8,
        Image = 16,
        Url = 32,
        AppName = 64,
        AppPath = 128,
        Meta = 256,
        Tag = 512
    }

    public interface MpIQueryInfo {
        public bool IsDescending { get;  set; }

        public MpContentSortType SortType { get; set; }

        public MpContentFilterType FilterFlags { get; set; }

        public int TagId { get; set; }

        public string SearchText { get;  set; }

        public int PageSize { get; set; }

        void NotifyQueryChanged();
    }
}
