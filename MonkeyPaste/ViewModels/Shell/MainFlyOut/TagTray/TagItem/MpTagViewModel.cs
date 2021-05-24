using System;
using System.Collections.Generic;
using System.Text;

namespace MonkeyPaste {
    public class MpTagViewModel : MpViewModelBase {
        public MpTag Tag { get; set; }

        public bool IsSelected { get; set; } = false;
        //public int CopyItemCount {
        //    get {
        //        if (Tag == null || Tag.CopyItemList == null) {
        //            return 0;
        //        }
        //        return Tag.CopyItemList.Count;
        //    }
        //}
        public MpTagViewModel() : this(null) { }

        public MpTagViewModel(MpTag tag) {
            Tag = tag;
        }
    }
}
