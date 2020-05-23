using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonkeyPaste {
    public class MpBoxModel {
        private int[] _values = new int[4];
        public int[] Values {
            get {
                return _values;
            }
            set {
                if(_values != value) {
                    _values = value;
                    Left = _values[0];
                    Right = _values[1];
                    Top = _values[2];
                    Bottom = _values[3];

                }
            }
        }

        public int Left {
            get {
                return _values[0];
            }
            set {
                if(_values[0] != value) {
                    _values[0] = value;
                }
            }
        }
        public int Right {
            get {
                return _values[1];
            }
            set {
                if (_values[1] != value) {
                    _values[1] = value;
                }
            }
        }
        public int Top {
            get {
                return _values[2];
            }
            set {
                if (_values[2] != value) {
                    _values[2] = value;
                }
            }
        }
        public int Bottom {
            get {
                return _values[3];
            }
            set {
                if (_values[3] != value) {
                    _values[3
] = value;
                }
            }
        }

        public MpBoxModel() { }
        public MpBoxModel(int val) {
            _values = new int[] { val, val, val, val };
        }
        public MpBoxModel(int l,int r,int t,int b) {
            _values = new int[] { l,r,t,b};
        }
    }
    public class MpPanelModel : MpTreeNode {
        //golden ratio
        public float gr = 1.618034f;
        //container panel model
        public MpPanelModel ContainerPanelModel;


        public MpPanelModel() {
            
        }
        public MpPanelModel(MpPanelModel containerPanelModel,string name) {
            ContainerPanelModel = containerPanelModel;
            Name = name;
            Rectangle ascr = MpScreenManager.Instance.GetScreenBoundsWithMouse();

        }
        public string Name { get; set; }

        private MpBoxModel _margin = new MpBoxModel();
        public MpBoxModel Margin {
            get {
                return _margin;
            }
            set {
                if(_margin != value) {
                    _margin = value;
                }
            }
        }

        private MpBoxModel _padding = new MpBoxModel();
        public MpBoxModel Padding {
            get {
                return _padding;
            }
            set {
                if (_padding != value) {
                    _padding = value;
                }
            }
        }

        private Rectangle _bounds = Rectangle.Empty;
        public Rectangle Bounds {
            get {
                return _bounds;
            }
            set {
                if(_bounds != value) {
                    _bounds = value;
                }
            }
        }
    }
}
