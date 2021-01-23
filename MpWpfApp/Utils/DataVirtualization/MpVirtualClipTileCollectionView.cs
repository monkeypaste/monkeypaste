using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace MpWpfApp {
    public class MpVirtualClipTileCollectionView : ListCollectionView {
        private MpClipTileViewModelDataSource _collection;

        public MpVirtualClipTileCollectionView(MpClipTileViewModelDataSource collection) : base(collection) {
            _collection = collection;
        }        

        protected override void RefreshOverride() {
            //m_collection.SetSortInternal(SortDescriptions);

            // Notify listeners that everything has changed
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));

            // The implementation of ListCollectionView saves the current item before updating the search
            // and restores it after updating the search. However, DataGrid, which is the primary client
            // of this view, does not use the current values. So, we simply set it to "beforeFirst"
            SetCurrent(null, -1);
        }
    }
}
