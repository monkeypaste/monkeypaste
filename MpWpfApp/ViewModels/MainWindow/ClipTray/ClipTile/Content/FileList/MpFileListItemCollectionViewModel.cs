using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MpWpfApp {
    public class MpFileListItemCollectionViewModel : MpUndoableObservableCollectionViewModel<MpFileListItemCollectionViewModel,MpFileListItemViewModel> {

        private MpClipTileViewModel _hostClipTileViewModel;
        public MpClipTileViewModel HostClipTileViewModel {
            get {
                return _hostClipTileViewModel;
            }
            set {
                if (_hostClipTileViewModel != value) {
                    _hostClipTileViewModel = value;
                    OnPropertyChanged(nameof(HostClipTileViewModel));
                }
            }
        }

        public MpFileListItemCollectionViewModel() : base() { }

        public MpFileListItemCollectionViewModel(MpClipTileViewModel hctvm) : this() {
            HostClipTileViewModel = hctvm;
        }

        public void ClearSubSelection() {
            foreach(var flvm in this) {
                flvm.IsSubSelected = false;
            }
        }
    }
}
