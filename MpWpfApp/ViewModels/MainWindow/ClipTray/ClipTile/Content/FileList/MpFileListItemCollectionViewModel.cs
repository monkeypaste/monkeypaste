using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MpWpfApp {
    public class MpFileListItemCollectionViewModel : MpUndoableObservableCollectionViewModel<MpFileListItemCollectionViewModel,MpFileListItemViewModel> {

        public MpFileListItemCollectionViewModel() : base() {

        }

        public void ClearSubSelection() {
            foreach(var flvm in this) {
                flvm.IsSubSelected = false;
            }
        }
    }
}
