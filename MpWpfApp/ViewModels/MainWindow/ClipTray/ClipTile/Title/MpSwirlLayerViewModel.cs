using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace MpWpfApp {
    public class MpSwirlLayerViewModel : MpViewModelBase<MpClipTileTitleSwirlViewModel> {
        #region Properties

        #region View Models

        #endregion

        #region Brushes
        private Brush _layerBrush = Brushes.Yellow;
        public Brush LayerBrush {
            get {
                if(Parent == null) {
                    return _layerBrush;
                }
                if(Parent.Parent.IsHovering && !Parent.Parent.Parent.IsExpanded) {
                    return new SolidColorBrush(Parent.Parent.ColorPallete[LayerId].ToWinMediaColor());
                }
                return _layerBrush;
            }
            set {
                if(_layerBrush != value) {
                    _layerBrush = value;
                    OnPropertyChanged_old(nameof(LayerBrush));
                }
            }
        }
        #endregion

        #region Appearance
        private double _layerOpacity = 1.0;
        public double LayerOpacity {
            get {
                return _layerOpacity;
            }
            set {
                if(_layerOpacity != value) {
                    _layerOpacity = value;
                    OnPropertyChanged_old(nameof(LayerOpacity));
                }
            }
        }
        #endregion

        #region Model

        private int _layerId = 0;
        public int LayerId {
            get {
                return _layerId;
            }
            set {
                if(_layerId != value) {
                    _layerId = value;
                    OnPropertyChanged_old(nameof(LayerId));
                }
            }
        }
        #endregion

        #endregion

        #region Public Methods
        public MpSwirlLayerViewModel() : base(null) {  }

        public MpSwirlLayerViewModel(MpClipTileTitleSwirlViewModel parent, int layerId, Brush layerBrush, double layerOpacity) : base(parent)  {
            LayerId = layerId;
            LayerBrush = layerBrush;
            LayerOpacity = layerOpacity;
        }
        #endregion
    }
}
