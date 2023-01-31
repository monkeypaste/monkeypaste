using System;
using System.Diagnostics;

namespace MonkeyPaste.Common {
    public class MpSizeChangeEventArgs : EventArgs {
        public MpSize OldSize { get; }
        public MpSize NewSize { get; }

        public MpSizeChangeEventArgs(MpSize oldSize, MpSize newSize) {
            OldSize = oldSize;
            NewSize = newSize;
        }
    }
    public class MpSize {
        //public static MpSize Parse(string text) {
        //    text = text.Trim();
        //    if(text.Contains())
        //}
        #region Statics

        public static MpSize Empty => new MpSize();

        #endregion

        #region Properties

        private double _width;
        public double Width {
            get => _width; 
            set {
                if(value < 0) {
                    Debugger.Break();
                    value = 0;
                }
                _width = value;
            }
        }

        private double _height;
        public double Height {
            get => _height;
            set {
                if (value < 0) {
                    Debugger.Break();
                    value = 0;
                }
                _height = value;
            }
        }

        #endregion

        #region Constructors

        public MpSize() { }
        public MpSize(double w, double h) {
            Width = w;
            Height = h;
        }

        #endregion
        #region Public Methods
        public bool IsEmpty(double maxThreshold = 0) {
            return Width <= maxThreshold && Height <= maxThreshold;
        }
        public override string ToString() {
            return $"Width: {Width} Height: {Height}";
        }
        #endregion
    }
}
