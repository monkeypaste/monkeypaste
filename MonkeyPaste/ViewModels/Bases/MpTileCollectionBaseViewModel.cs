using System.Collections.ObjectModel;

namespace MonkeyPaste {
    public abstract class MpTileCollectionBaseViewModel<TCollectionItems> : MpViewModelBase {
        public ObservableCollection<TCollectionItems> Items { get; set; }

    }
}
