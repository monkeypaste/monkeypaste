using System;
using System.Collections.Generic;
using System.Text;

namespace MonkeyPaste {
    public interface MpISidebarItemViewModel {
        double DefaultSidebarWidth { get; }
        double SidebarWidth { get; set; }
        bool IsSidebarVisible { get; set; }
        MpISidebarItemViewModel NextSidebarItem { get; }
        MpISidebarItemViewModel PreviousSidebarItem { get; }
    }
}
