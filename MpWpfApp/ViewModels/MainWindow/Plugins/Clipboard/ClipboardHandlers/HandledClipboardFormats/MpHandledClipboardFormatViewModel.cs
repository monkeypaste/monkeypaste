using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MonkeyPaste;
using MonkeyPaste.Plugin;

namespace MpWpfApp {
    public class MpHandledClipboardFormatViewModel :
        MpSelectorViewModelBase<MpClipboardHandlerItemViewModel,MpClipboardFormatPresetViewModel>,
        MpISelectableViewModel,
        MpIHoverableViewModel {
        public bool IsSelected { get; set; }
        public bool IsHovering { get; set; }
    }
}
