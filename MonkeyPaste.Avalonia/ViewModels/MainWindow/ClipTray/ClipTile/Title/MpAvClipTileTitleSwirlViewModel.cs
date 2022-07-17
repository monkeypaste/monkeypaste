using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MonkeyPaste;
using MonkeyPaste.Common;
namespace MonkeyPaste.Avalonia {
    public class MpAvClipTileTitleSwirlViewModel : MpViewModelBase<MpAvClipTileViewModel> {
        #region Private Variables

        #endregion

        #region Properties

        #region View Models

        public ObservableCollection<string> HexColors { get; set; } = new ObservableCollection<string>();
        public ObservableCollection<double> Opacities { get; set; } = new ObservableCollection<double>();

        #endregion

        #region Brushes

        #endregion

        #region State

        public bool HasUserDefinedColor {
            get {
                if(Parent == null || Parent.CopyItem == null || string.IsNullOrEmpty(Parent.CopyItem.ItemColor)) {
                    return false;
                }
                return true;
            }
        }

        #endregion

        #region Model

        #endregion

        #endregion

        #region Public Methods
        public MpAvClipTileTitleSwirlViewModel() : base(null) { }

        public MpAvClipTileTitleSwirlViewModel(MpAvClipTileViewModel parent) : base(parent) {
        }

        public async Task InitializeAsync() {
            //if(Parent.IsPlaceholder || IsBusy) {
            //    return;
            //}
            IsBusy = true;

            if(Parent.IconId > 0 && !HasUserDefinedColor) {                
                var ivm = MpIconCollectionViewModel.Instance.IconViewModels.FirstOrDefault(x => x.IconId == Parent.IconId);
                if(ivm == null) {
                    var icon = await MpDb.GetItemAsync<MpIcon>(Parent.IconId);
                    HexColors = new ObservableCollection<string>(icon.HexColors);
                } else {
                    HexColors = ivm.PrimaryIconColorList;
                }                
            } else if (HasUserDefinedColor) {
                HexColors = new ObservableCollection<string>(Enumerable.Repeat(Parent.CopyItemHexColor, 5));
            } else {
                var tagColors = await MpDataModelProvider.GetTagColorsForCopyItemAsync(Parent.CopyItemId);
                tagColors.ForEach(x => HexColors.Insert(0, x));
            }

            if(HexColors.Count == 0) {
                HexColors = new ObservableCollection<string>(Enumerable.Repeat(MpColorHelpers.GetRandomHexColor(), 5));
            }
            HexColors = new ObservableCollection<string>(HexColors.Take(5));
            Opacities = new ObservableCollection<double>(Enumerable.Repeat((double)MpRandom.Rand.Next(40, 120) / 255, 5));

            IsBusy = false;
        }

        #endregion
    }
}
