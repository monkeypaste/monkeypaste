using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;

namespace MonkeyPaste {
    public class MpCopyItemTagAssociatedItemViewModel : MpViewModelBase {
        #region Properties
        public MpCopyItem CopyItem { get; set; }

        public MpTag Tag { get; set; }

        public bool IsAssociated { get; set; }

        public bool IsEnabled {
            get {
                if(Tag == null) {
                    return false;
                }

                return Tag.Id != MpTag.AllTagId && Tag.Id != MpTag.RecentTagId;
            }
        }

        #endregion

        #region Public Methods
        public MpCopyItemTagAssociatedItemViewModel() : base() { }

        public MpCopyItemTagAssociatedItemViewModel(MpCopyItem clip, MpTag tag) : this() {
            PropertyChanged += MpCopyItemTagAssociatedItemViewModel_PropertyChanged;
            CopyItem = clip;
            Tag = tag;
            Task.Run(Initialize);
        }

        private async void MpCopyItemTagAssociatedItemViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            await Task.Delay(1);
            switch(e.PropertyName) {
                default:
                    break;
            }
        }
        #endregion

        #region Private Methods
        private async Task Initialize() {
            await UpdateAssocation();

            Tag.CopyItemList = await MpCopyItem.GetAllCopyItemsByTagId(Tag.Id);
            Tag.Color = await MpColor.GetColorByIdAsync(Tag.ColorId);
        }

        private async Task UpdateAssocation() {
            if (CopyItem == null || Tag == null) {
                return;
            }

            IsAssociated = await Tag.IsLinkedWithCopyItemAsync(CopyItem);
        }
        #endregion

        #region Commands
        public ICommand ToggleAssociationCommand => new Command(
            async () => {
                if(IsAssociated) {
                    await Tag.UnlinkWithCopyItemAsync(CopyItem);
                } else {
                    await Tag.LinkWithCopyItemAsync(CopyItem);
                }
                await UpdateAssocation();
            },
            () => {
                return CopyItem != null && Tag != null && IsEnabled;
            });
        #endregion
    }
}
