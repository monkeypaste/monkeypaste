using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MpWpfApp {
    public class MpPagingUndoableObservableCollection<T,I> : MpUndoableObservableCollectionViewModel<T,I> where I : MpViewModelBase {
        #region Private Variables
        private int _pageSize = 10;
        #endregion

        public MpPagingUndoableObservableCollection() {
            for (int i = 0; i < _pageSize; i++) {
                //base.Add(new)
            }
        }

        public MpPagingUndoableObservableCollection(int pageSize) : this() {
            _pageSize = pageSize;
        }

        public new void Add(I item) {

        }
    }
}
