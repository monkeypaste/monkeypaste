using System;
using System.Collections.Generic;
using System.Text;

namespace MonkeyPaste {
    public interface MpISidebarItemViewModel : MpISelectableViewModel {
        double DefaultSidebarWidth { get; }
        double DefaultSidebarHeight { get; }
        double SidebarWidth { get; set; }
        double SidebarHeight { get; set; }
    }
}
