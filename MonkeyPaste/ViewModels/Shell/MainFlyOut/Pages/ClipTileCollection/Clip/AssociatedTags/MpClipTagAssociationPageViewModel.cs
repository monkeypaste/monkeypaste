using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Threading.Tasks;
using System.Linq;
using Xamarin.Essentials;

namespace MonkeyPaste {
    public class MpCopyItemTagAssociationPageViewModel : MpViewModelBase {
        #region Properties
        public MpCopyItem CopyItem { get; set; }

        public ObservableCollection<MpCopyItemTagAssociatedItemViewModel> CopyItemTagAssociatedItemViewModels { get; set; }

        public double PageWidth {
            get {
                return DeviceDisplay.MainDisplayInfo.Width * 0.75;
            }
        }
        #endregion

        #region Public Methods
        public MpCopyItemTagAssociationPageViewModel() : base() { }

        public MpCopyItemTagAssociationPageViewModel(MpCopyItem clip) {
            CopyItem = clip;
            Task.Run(Initialize);
        }
        #endregion

        #region Private Methods
        private async Task Initialize() {
            var tags = await MpDb.Instance.GetItemsAsync<MpTag>();
            CopyItemTagAssociatedItemViewModels = 
                new ObservableCollection<MpCopyItemTagAssociatedItemViewModel>(
                    tags.Select(x => CreateCopyItemTagAssociatedViewModel(x)));
        }

        private MpCopyItemTagAssociatedItemViewModel CreateCopyItemTagAssociatedViewModel(MpTag tag) {
            var nctavm = new MpCopyItemTagAssociatedItemViewModel(CopyItem, tag);
            return nctavm;
        }
        #endregion
    }
}
