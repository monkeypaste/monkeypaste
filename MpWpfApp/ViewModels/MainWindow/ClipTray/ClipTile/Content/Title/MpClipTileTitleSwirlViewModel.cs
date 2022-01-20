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
    public class MpClipTileTitleSwirlViewModel : MpViewModelBase<MpContentItemViewModel> {
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

        public MpClipTileTitleSwirlViewModel(MpContentItemViewModel parent) : base(parent) { }

        public async Task InitializeAsync() {
            if(Parent.IsPlaceholder) {
                return;
            }
            await MpHelpers.RunOnMainThreadAsync(() => {
                var cl = Parent.ColorPallete;
                for (int i = 0; i < cl.Length; i++) {
                    var scb = new SolidColorBrush(cl[i].ToWinMediaColor());
                    if (i < Swirls.Count) {
                        Swirls[i].LayerId = i;
                        Swirls[i].LayerBrush = scb;
                        Swirls[i].LayerOpacity = (double)MpHelpers.Rand.Next(40, 120) / 255;
                    } else {
                        Swirls.Add(
                            new MpSwirlLayerViewModel(
                                this,
                                i,
                                scb,
                                (double)MpHelpers.Rand.Next(40, 120) / 255));
                    }
                }
            });
        }
        public void ForceBrush(Brush forcedBrush) {
            foreach(var slvm in Swirls) {
                slvm.LayerBrush = forcedBrush;
                slvm.LayerOpacity = (double)MpHelpers.Rand.Next(40, 120) / 255;
            }
        }

        public override void Dispose() {
            base.Dispose();
        }
        #endregion

        #region Private Methods

        #endregion
    }
}
