using System;
using System.Collections.Generic;
using System.Text;

namespace MonkeyPaste {
    public class MpSearchInfo {
        public string SearchText { get; set; } = string.Empty;

        public bool IsDescending { get; set; } = false;

        public bool IsCaseSensitive { get; set; } = false;

        public MpPagingInfo PagingInfo { get; set; }
    }
}
