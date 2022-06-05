using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using MonkeyPaste;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Wpf;
namespace MpWpfApp {
    public class MpClipTileTitleSwirlViewModel : MpViewModelBase<MpClipTileViewModel> {
        #region Private Variables

        #endregion

        #region Properties

        #region View Models

        public ObservableCollection<MpSwirlLayerViewModel> Swirls { get; set; } = new ObservableCollection<MpSwirlLayerViewModel>();

        public MpSwirlLayerViewModel SwirlLayer0 {
            get {
                if(Swirls.Count <= 0) {
                    return null;
                }
                return Swirls[0];
            }
            set {
                if(Swirls.Count > 0 && Swirls[0] != value) {
                    Swirls[0] = value;
                    OnPropertyChanged(nameof(SwirlLayer0));
                }
            }
        }

        public MpSwirlLayerViewModel SwirlLayer1 {
            get {
                if (Swirls.Count <= 1) {
                    return null;
                }
                return Swirls[1];
            }
            set {
                if (Swirls.Count > 1 && Swirls[1] != value) {
                    Swirls[1] = value;
                    OnPropertyChanged(nameof(SwirlLayer1));
                }
            }
        }

        public MpSwirlLayerViewModel SwirlLayer2 {
            get {
                if (Swirls.Count <= 2) {
                    return null;
                }
                return Swirls[2];
            }
            set {
                if (Swirls.Count > 2 && Swirls[2] != value) {
                    Swirls[2] = value;
                    OnPropertyChanged(nameof(SwirlLayer2));
                }
            }
        }

        public MpSwirlLayerViewModel SwirlLayer3 {
            get {
                if (Swirls.Count <= 3) {
                    return null;
                }
                return Swirls[3];
            }
            set {
                if (Swirls.Count > 3 && Swirls[3] != value) {
                    Swirls[3] = value;
                    OnPropertyChanged(nameof(SwirlLayer3));
                }
            }
        }
        #endregion

        #region Brushes

        #endregion

        #region State

        public bool IsAnyBusy => IsBusy || Swirls.Any(x => x.IsBusy);
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
        public MpClipTileTitleSwirlViewModel() : base(null) { }

        public MpClipTileTitleSwirlViewModel(MpClipTileViewModel parent) : base(parent) {
        }

        public async Task InitializeAsync() {
            if(Parent.IsPlaceholder || IsBusy) {
                return;
            }
            IsBusy = true;

            var icon = MpDb.GetItem<MpIcon>(Parent.IconId);

            var pallete = new List<string>{
                    icon.HexColor1,
                    icon.HexColor3,
                    icon.HexColor3,
                    icon.HexColor4,
                    icon.HexColor5
                };

            if (HasUserDefinedColor) {
                pallete = Enumerable.Repeat(Parent.CopyItemHexColor, 5).ToList();
            } else {
                var tagColors = await MpDataModelProvider.GetTagColorsForCopyItem(Parent.CopyItemId);
                pallete.InsertRange(0, tagColors);
            }
            pallete = pallete.Take(5).ToList();

            Swirls.Clear();

            for (int i = 0; i < pallete.ToList().Count; i++) {
                var scb = new SolidColorBrush(pallete[i].ToWinMediaColor());
                MpSwirlLayerViewModel slvm = CreateSwirlLayerViewModel(i, scb);

                Swirls.Add(slvm);
            }

            OnPropertyChanged(nameof(SwirlLayer0));
            OnPropertyChanged(nameof(SwirlLayer1));
            OnPropertyChanged(nameof(SwirlLayer2));
            OnPropertyChanged(nameof(SwirlLayer3));

            IsBusy = false;
        }

        private MpSwirlLayerViewModel CreateSwirlLayerViewModel(int layerId,SolidColorBrush b) {
            var slvm = new MpSwirlLayerViewModel(
                            this,
                            layerId,
                            b,
                            (double)MpHelpers.Rand.Next(40, 120) / 255);
            return slvm;

        }

        public override void Dispose() {
            base.Dispose();
        }
        #endregion

        #region Private Methods

        #endregion
    }
}
