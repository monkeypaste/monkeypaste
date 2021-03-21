using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace MpWpfApp {
    public class MpClipTileTitleSwirlViewModel : MpUndoableObservableCollectionViewModel<MpClipTileTitleSwirlViewModel,MpSwirlLayerViewModel> {
        #region Private Variables

        #endregion

        #region Properties

        #region View Models
        public MpClipTileViewModel _clipTileViewModel = null;
        public MpClipTileViewModel ClipTileViewModel {
            get {
                return _clipTileViewModel;
            }
            set {
                if (_clipTileViewModel != value) {
                    _clipTileViewModel = value;
                    OnPropertyChanged(nameof(ClipTileViewModel));
                }
            }
        }

        public MpAppViewModel _appViewModel = null;
        public MpAppViewModel AppViewModel {
            get {
                return _appViewModel;
            }
            set {
                if(_appViewModel != value) {
                    _appViewModel = value;
                    OnPropertyChanged(nameof(AppViewModel));
                }
            }
        }

        public MpSwirlLayerViewModel SwirlLayer0 {
            get {
                if(this.Count <= 0) {
                    return null;
                }
                return this[0];
            }
            set {
                if(this.Count > 0 && this[0] != value) {
                    this[0] = value;
                    OnPropertyChanged(nameof(SwirlLayer0));
                }
            }
        }

        public MpSwirlLayerViewModel SwirlLayer1 {
            get {
                if (this.Count <= 1) {
                    return null;
                }
                return this[1];
            }
            set {
                if (this.Count > 1 && this[1] != value) {
                    this[1] = value;
                    OnPropertyChanged(nameof(SwirlLayer1));
                }
            }
        }

        public MpSwirlLayerViewModel SwirlLayer2 {
            get {
                if (this.Count <= 2) {
                    return null;
                }
                return this[2];
            }
            set {
                if (this.Count > 2 && this[2] != value) {
                    this[2] = value;
                    OnPropertyChanged(nameof(SwirlLayer2));
                }
            }
        }

        public MpSwirlLayerViewModel SwirlLayer3 {
            get {
                if (this.Count <= 3) {
                    return null;
                }
                return this[3];
            }
            set {
                if (this.Count > 3 && this[3] != value) {
                    this[3] = value;
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
        public MpClipTileTitleSwirlViewModel() : base() { }

        public MpClipTileTitleSwirlViewModel(MpClipTileViewModel ctvm) : this() {
            ClipTileViewModel = ctvm;
            AppViewModel = new MpAppViewModel(ClipTileViewModel.CopyItem.App);
            var randomColorList = MpHelpers.Instance.GetRandomizedList<MpColor>(AppViewModel.PrimaryIconColorList);
            for (int i = 0; i < randomColorList.Count; i++) {
                var c = AppViewModel.PrimaryIconColorList[i];
                this.Add(
                    new MpSwirlLayerViewModel(
                        i,
                        c.ColorBrush,
                        (double)MpHelpers.Instance.Rand.Next(40, 120) / 255));
            }
        }

        public void TitleSwirlCanvas_Loaded(object sender, RoutedEventArgs args) {
            
        }

        public void ForceBrush(Brush forcedBrush, int forceLayerIdx=-1) {
            foreach(var slvm in this) {
                if(forceLayerIdx >= 0 && this.IndexOf(slvm) == forceLayerIdx) {

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
