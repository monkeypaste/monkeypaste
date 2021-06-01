using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace MonkeyPaste {
    public abstract class MpTileCollectionBaseViewModel<TCollectionItems> : MpViewModelBase {
        public ObservableCollection<TCollectionItems> Items { get; set; }

    }
}
