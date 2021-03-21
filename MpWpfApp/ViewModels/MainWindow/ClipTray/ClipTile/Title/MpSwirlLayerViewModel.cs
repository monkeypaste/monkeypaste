using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace MpWpfApp {
    public class MpSwirlLayerViewModel : MpUndoableViewModelBase<MpSwirlLayerViewModel> {

        #region Properties

        #region View Models

        #endregion

        #region Brushes
        private Brush _layerBrush = MpHelpers.Instance.GetRandomBrushColor();
        public Brush LayerBrush {
            get {
                return _layerBrush;
            }
            set {
                if(_layerBrush != value) {
                    _layerBrush = value;
                    OnPropertyChanged(nameof(LayerBrush));
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
                    OnPropertyChanged(nameof(LayerOpacity));
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
                    OnPropertyChanged(nameof(LayerId));
                }
            }
        }
        #endregion

        #endregion

        #region Public Methods
        public MpSwirlLayerViewModel() : base() {  }

        public MpSwirlLayerViewModel(int layerId, Brush layerBrush, double layerOpacity) : this()  {
            LayerId = layerId;
            LayerBrush = layerBrush;
            LayerOpacity = layerOpacity;
        }
        #endregion
    }
}
