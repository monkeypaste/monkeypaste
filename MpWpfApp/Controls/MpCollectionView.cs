using AlphaChiTech.Virtualization;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace MpWpfApp {
    public class MpCollectionView : ListCollectionView {
        VirtualizingObservableCollection<MpClipTileViewModel> m_collection;

        public MpCollectionView(VirtualizingObservableCollection<MpClipTileViewModel> collection) : base(collection) {
            m_collection = collection;
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
