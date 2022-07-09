using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonkeyPaste.Avalonia {
    public class MpAvClipTrayViewModel : MpSelectorViewModelBase<object,MpAvClipTileViewModel>,
        MpIBootstrappedItem {
        #region Private Variables

        #endregion

        #region Statics

        private static MpAvClipTrayViewModel _instance;
        public static MpAvClipTrayViewModel Instance => _instance ?? (_instance = new MpAvClipTrayViewModel());

        #endregion

        #region Properties

        #region MpIBoostrappedItem Implementation

        string MpIBootstrappedItem.Label => "Content Tray";
        #endregion

        #endregion

        #region Constructors

        private MpAvClipTrayViewModel() : base() { }

        #endregion

        #region Public Methods

        public async Task InitializeAsync() {
            IsBusy = true;

            for(int i = 1;i <= 100;i++) {
                var test_ctvm = await CreateClipTileViewModel(
                    new MpCopyItem() {
                        Id = i,
                        ItemType = MpCopyItemType.Text,
                        ItemData = "This is test "+i,
                        Title = "Test"+i
                    });
                Items.Add(test_ctvm);
            }

            SelectedItem = Items[0];

            OnPropertyChanged(nameof(Items));
            

            IsBusy = false;
        }

        

        #endregion

        #region Private Methods

        private async Task<MpAvClipTileViewModel> CreateClipTileViewModel(MpCopyItem ci) {
            MpAvClipTileViewModel ctvm = new MpAvClipTileViewModel(this);
            await ctvm.InitializeAsync(ci);
            return ctvm;
        }

        #endregion

        #region Commands

        #endregion
    }
}
