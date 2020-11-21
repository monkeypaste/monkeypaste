using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MpWpfApp {
    public class MpSubTextTokenCollectionViewModel : MpObservableCollectionViewModel<MpSubTextTokenViewModel> {
        #region View Models
        private MpClipTileViewModel _clipTileViewModel = null;
        public MpClipTileViewModel ClipTileViewModel {
            get {
                return _clipTileViewModel;
            }
            set {
                if(_clipTileViewModel != value) {
                    _clipTileViewModel = value;
                    OnPropertyChanged(nameof(ClipTileViewModel));
                }
            }
        }
        #endregion

        #region Public Methods
        public MpSubTextTokenCollectionViewModel(MpClipTileViewModel ctvm) {
            ClipTileViewModel = ctvm;
        }
        #endregion
    }
}
