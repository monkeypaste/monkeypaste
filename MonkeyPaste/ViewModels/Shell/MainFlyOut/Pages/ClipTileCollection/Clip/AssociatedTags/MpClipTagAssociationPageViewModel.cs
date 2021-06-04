using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Threading.Tasks;
using System.Linq;
using Xamarin.Essentials;

namespace MonkeyPaste {
    public class MpClipTagAssociationPageViewModel : MpViewModelBase {
        #region Properties
        public MpClip Clip { get; set; }

        public ObservableCollection<MpClipTagAssociatedItemViewModel> ClipTagAssociatedItemViewModels { get; set; }

        public double PageWidth {
            get {
                return DeviceDisplay.MainDisplayInfo.Width * 0.75;
            }
        }
        #endregion

        #region Public Methods
        public MpClipTagAssociationPageViewModel() : base() { }

        public MpClipTagAssociationPageViewModel(MpClip clip) {
            Clip = clip;
            Task.Run(Initialize);
        }
        #endregion

        #region Private Methods
        private async Task Initialize() {
            var tags = await MpDb.Instance.GetItems<MpTag>();
            ClipTagAssociatedItemViewModels = 
                new ObservableCollection<MpClipTagAssociatedItemViewModel>(
                    tags.Select(x => CreateClipTagAssociatedViewModel(x)));
        }

        private MpClipTagAssociatedItemViewModel CreateClipTagAssociatedViewModel(MpTag tag) {
            var nctavm = new MpClipTagAssociatedItemViewModel(Clip, tag);
            return nctavm;
        }
        #endregion
    }
}
