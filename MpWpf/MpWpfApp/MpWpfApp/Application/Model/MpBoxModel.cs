using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MpWpfApp {
    public class MpBoxModel : MpModel {
        private int[] _values = new int[4];
        public int[] Values {
            get {
                return _values;
            }
            set {
                if (_values != value) {
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
                if (_values[0] != value) {
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
                    _values[3] = value;
                }
            }
        }

        public MpBoxModel() { _values = new int[4]; }
        public MpBoxModel(int val) {
            _values = new int[] { val, val, val, val };
        }
        public MpBoxModel(int l, int r, int t, int b) {
            _values = new int[] { l, r, t, b };
        }
    }
}
