using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MonkeyPaste;

namespace MonkeyPaste.Avalonia {
    public class MpAvClipTileViewModel : MpViewModelBase<MpAvClipTrayViewModel>,
        MpISelectableViewModel,
        MpIHoverableViewModel {
        #region Properties

        #region MpISelectableViewModel Implementation
        public bool IsSelected { get; set; }
        public DateTime LastSelectedDateTime { get; set; }

        #endregion

        #region MpIHoverableViewModel Implementation
        public bool IsHovering { get; set; }

        #endregion

        #region State

        #endregion

        #region Model

        public string CopyItemData {
            get {
                if(CopyItem == null) {
                    return string.Empty;
                }
                return CopyItem.ItemData;
            }
            set {
                if (CopyItem.ItemData != value) {
                    CopyItem.ItemData = value;
                    HasModelChanged = true;
                    OnPropertyChanged(nameof(CopyItemData));
                }
            }
        }

        public MpCopyItem CopyItem { get; set; }

        #endregion

        #endregion

        #region Contructors
        public MpAvClipTileViewModel() : base(null) { }

        public MpAvClipTileViewModel(MpAvClipTrayViewModel parent) : base(parent) {
            PropertyChanged += MpAvClipTileViewModel_PropertyChanged;
        }

        #endregion

        #region Public Methods

        public async Task InitializeAsync(MpCopyItem ci, int queryOffsetIdx = -1) {
            IsBusy = true;

            await Task.Delay(1);

            CopyItem = ci;

            IsBusy = false;
        }

        #endregion

        #region Private Methods

        private void MpAvClipTileViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            
        }


        #endregion
    }
}
