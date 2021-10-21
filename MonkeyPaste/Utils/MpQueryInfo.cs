using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace MonkeyPaste {
    public class MpQueryInfo {
        public virtual bool IsDescending { get; set; } = true;
        public virtual MpContentSortType SortType { get; set; } = MpContentSortType.CopyDateTime;

        public virtual int TagId { get; set; } = MpTag.RecentTagId;

        public virtual string SearchText { get; set; } = string.Empty;

        public virtual MpContentFilterType FilterFlags { get; set; } = MpContentFilterType.None;

        public void NotifyQueryChanged() {
            MpMessenger.Instance.Send<bool>(true);
        }
    }
}
