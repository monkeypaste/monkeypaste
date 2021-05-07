
using MonkeyPaste.Models;
using System;

namespace MonkeyPaste.ViewModels {
    public class MpCopyItemViewModel : Base.MpViewModelBase {
        public MpCopyItemViewModel(MpCopyItem item) => CopyItem = item;

        public event EventHandler ItemStatusChanged;

        public MpCopyItem CopyItem { get; private set; }

        public string StatusText => CopyItem.Title;
    }
}

