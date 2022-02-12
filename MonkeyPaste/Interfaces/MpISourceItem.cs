using System;
using System.Collections.Generic;
using System.Text;

namespace MonkeyPaste {
    public interface MpISourceItem {
        //MpIcon SourceIcon { get; }
        int IconId { get; }
        string SourcePath { get; }
        string SourceName { get; }

        int RootId { get; }

        bool IsUrl { get; }

        bool IsRejected { get; }
        bool IsSubRejected { get; }
    }

}
