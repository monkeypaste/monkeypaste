using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MpWpfApp {
    public class MpFileListItemCollectionViewModel : MpClipTileViewModel {

        public ObservableCollection<MpFileListItemViewModel> FileItems = new ObservableCollection<MpFileListItemViewModel>();
        
        public MpFileListItemCollectionViewModel() : base(null,null) { }

        public MpFileListItemCollectionViewModel(MpClipTileViewModel hctvm) : base(hctvm,hctvm.CopyItem) {
        }
    }
}
