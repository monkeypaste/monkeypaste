using System.Drawing;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.Diagnostics;
using System;
using System.Linq;
using MonkeyPaste.Common.Utils.Extensions;

namespace MonkeyPaste.Common {
    public class MpRect : MpShape {

        #region Private Variables

        private double _left, _top, _right, _bottom;

        #endregion

        #region Statics
        public static MpRect Empty => new MpRect();

        public static MpRect Union(MpRect a, MpRect b) {
            double x1 = Math.Min(a.X, b.X);
            double x2 = Math.Max(a.X + a.Width, b.X + b.Width);
            double y1 = Math.Min(a.Y, b.Y);
            double y2 = Math.Max(a.Y + a.Height, b.Y + b.Height);

            return new MpRect(x1, y1, x2 - x1, y2 - y1);
        }

        public static MpRect ParseJson(string jsonStr) {
            ///quill json '{ left: Number, top: Number, height: Number, width: Number }'
            var qr = MpJsonObject.DeserializeObject<MpJsonRect>(jsonStr);
            var r = new MpRect(qr.left, qr.top, qr.width, qr.height);
            return r;
        }
        #endregion

        #region Properties

        #region Read-Only Properties

        public MpPoint TopLeft => new MpPoint(Left, Top);
        public MpPoint TopRight => new MpPoint(Right, Top);
        public MpPoint BottomLeft => new MpPoint(Left, Bottom);
        public MpPoint BottomRight => new MpPoint(Right, Bottom);

        public override MpPoint[] Points => new MpPoint[] { TopLeft, TopRight, BottomRight, BottomLeft };

        public double Width => _right - _left;

        public double Height => _bottom - _top;

        public MpSize Size => new MpSize(Width, Height);

        public MpPoint Location => new MpPoint(X, Y);

        #endregion

        #region Drawing Properties

        public double RadiusX { get; set; } = 0;
        public double RadiusY { get; set; } = 0;
        public string BoxShadows { get; set; } = string.Empty;

        #endregion


        public double X {
            get => Left;
            set => Left = value;
        }
        public double Y {
            get => Top;
            set => Top = value;
        }

        public double Left {
            get => _left;
            set {
                if(_left != value) {
                    _right = value + Width;
                    _left = value;
                }
            }
        }

        public double Top {
            get => _top;
            set {
                if (_top != value) {
                    _bottom = value + Height;
                    _top = value;
                }
            }
        }

        public double Right {
            get => _right;
            set {
                if (_right != value) {
                    _left = value - Width;
                    _right = value;
                }
            }
        }

        public double Bottom {
            get => _bottom;
            set {
                if (_bottom != value) {
                    _top = value - Height;
                    _bottom = value;
                }
            }
        }

        #endregion

        #region Constructors

        public MpRect() : base() { }

        public MpRect(double x, double y, double w, double h) : this(new MpPoint(x, y), new MpSize(w, h)) { }

        public MpRect(MpPoint location, MpSize size) : this() {
            _left = location.X;
            _top = location.Y;
            _right = _left + size.Width;
            _bottom = _top + size.Height;
        }

        #endregion

        #region Public Methods

        public override string ToString() {
            return $"X:{X} Y:{Y} Width: {Width} Height: {Height}";
        }

        

        #endregion

        #region Private Methods


        #endregion
    }
}
