using System;
using System.Linq;

namespace MonkeyPaste.Common {
    public class MpRect2 : MpShape {
        private MpPoint[] _points;
        public override MpPoint[] Points => _points;//throw new NotImplementedException();
        public static MpRect2 Empty => new MpRect2();

        #region Statics

        public static MpRect2 Union(MpRect2 a, MpRect2 b) {
            double x1 = Math.Min(a.X, b.X);
            double x2 = Math.Max(a.X + a.Width, b.X + b.Width);
            double y1 = Math.Min(a.Y, b.Y);
            double y2 = Math.Max(a.Y + a.Height, b.Y + b.Height);

            return new MpRect2(x1, y1, x2 - x1, y2 - y1);
        }

        public static MpRect2 ParseJson(string jsonStr) {
            ///quill json '{ left: Number, top: Number, height: Number, width: Number }'
            var qr = MpJsonObject.DeserializeObject<MpJsonRect>(jsonStr);
            var r = new MpRect2(qr.left, qr.top, qr.width, qr.height);
            return r;
        }
        #endregion
        public MpPoint Location {
            get => _points[0];
            set => _points[0] = value;
        }

        public MpPoint TopLeft => new MpPoint(Left, Top);
        public MpPoint TopRight => new MpPoint(Right, Top);

        public MpPoint BottomLeft => new MpPoint(Left, Bottom);
        public MpPoint BottomRight => new MpPoint(Right, Bottom);

        public double X {
            get => _points[0].X;
            set => _points[0].X = value;
        }
        public double Y {
            get => _points[0].Y;
            set => _points[0].Y = value;
        }

        public MpSize Size {
            get => new MpSize(_points[1].X, _points[1].Y);
            set => _points[1] = new MpPoint(value.Width, value.Height);
        }

        public double Left {
            get => X;
            set => X = value;
        }

        public double Top {
            get => Y;
            set => Y = value;
        }

        public double Right {
            get => X + Width;
            set {
                if (value - X < 0) {
                    //swap left & right
                    double temp = X;
                    X = value;
                    Right = temp;
                } else {
                    Width = value - X;
                }
            }
        }

        public double Bottom {
            get => Y + Height;
            set {
                if (value - Top < 0) {
                    //swap left & right
                    double temp = Y;
                    Y = value;
                    Bottom = temp;
                } else {
                    Height = value - Y;
                }
            }
        }

        public double Width {
            get => Size.Width;
            set => Size.Width = value;
        }

        public double Height {
            get => Size.Height;
            set => Size.Height = value;
        }


        public MpRect2() : base() {
            _points = Enumerable.Repeat(new MpPoint(), 4).ToArray();
        }
        public MpRect2(MpPoint location, MpSize size) {
           _points = new MpPoint[] { location, new MpPoint(size.Width, size.Height) };
        }

        public MpRect2(double x, double y, double w, double h) : this(new MpPoint(x, y), new MpSize(w, h)) { }

        public bool Contains(MpPoint p) {
            return p.X >= Left && p.X <= Right &&
                   p.Y >= Top && p.Y <= Bottom;
        }

        public void Union(MpRect2 b) {
            Left = Math.Min(Left, b.Left);
            Top = Math.Min(Top, b.Top);
            Right = Math.Max(Right, b.Right);
            Bottom = Math.Max(Bottom, b.Bottom);
        }

        public override string ToString() {
            return $"X:{Location.X} Y:{Location.Y} Width: {Width} Height: {Height}";
        }

    }
}
