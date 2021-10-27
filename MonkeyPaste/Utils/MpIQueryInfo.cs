using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace MonkeyPaste {
    public interface MpIQueryInfo {
        public bool IsDescending { get;  set; }

        public MpContentSortType SortType { get; set; }

        public int TagId { get; set; }

        public string SearchText { get;  set; }

        public MpContentFilterType FilterFlags { get;  set; }

        public int PageSize { get; set; }

        void NotifyQueryChanged();
    }
}
