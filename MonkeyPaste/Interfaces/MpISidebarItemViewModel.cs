using System;
using System.Collections.Generic;
using System.Text;

namespace MonkeyPaste {
    public interface MpISidebarItemViewModel {
        double DefaultSidebarWidth { get; }
        bool IsSidebarVisible { get; set; }
        MpISidebarItemViewModel NextSidebarItem { get; set; }
        MpISidebarItemViewModel PreviousSidebarItem { get; set; }
    }
}
