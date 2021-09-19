using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using MonkeyPaste;

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

        #region Business Logic

        #endregion

        #endregion

        #region Public Methods
        public MpClipTileTitleSwirlViewModel() : base(null) { }

        public MpClipTileTitleSwirlViewModel(MpClipTileViewModel ctvm) : base(ctvm) {
            var icon = Parent.HeadItem.CopyItem.Source.App.Icon;
            var cl = new List<string>() { icon.HexColor1, icon.HexColor2, icon.HexColor3, icon.HexColor4, icon.HexColor5 };
            var randomColorList = MpHelpers.Instance.GetRandomizedList<string>(cl);
            for (int i = 0; i < randomColorList.Count; i++) {
                var c = cl[i].ToSkColor().ToWinColor();
                Swirls.Add(
                    new MpSwirlLayerViewModel(
                        i,
                        new SolidColorBrush(c),
                        (double)MpHelpers.Instance.Rand.Next(40, 120) / 255));
            }
        }

        public void TitleSwirlCanvas_Loaded(object sender, RoutedEventArgs args) {
            
        }

        public void ForceBrush(Brush forcedBrush, int forceLayerIdx=-1) {
            foreach(var slvm in Swirls) {
                if(forceLayerIdx >= 0 && Swirls.IndexOf(slvm) == forceLayerIdx) {

                } else if(forceLayerIdx < 0) {
                    slvm.LayerBrush = forcedBrush;
                    slvm.LayerOpacity = (double)MpHelpers.Instance.Rand.Next(40, 120) / 255;
                }
            }
        }
        #endregion

        #region Private Variables

        #endregion

    }
}
