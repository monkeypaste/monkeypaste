using System.Drawing;
using System;
using System.Linq;
using System.Collections.Generic;

namespace MonkeyPaste.Common {
    public class MpRect : MpShape {
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
            var rectVals = MpJsonObject.DeserializeObject<List<double>>(jsonStr);
            return new MpRect(rectVals[0], rectVals[1], rectVals[2], rectVals[3]);
        }

        #endregion

        #region Properties

        public MpPoint Location {
            get => Points[0];
            set => Points[0] = value;
        }

        public MpPoint TopLeft => new MpPoint(Left, Top);
        public MpPoint TopRight => new MpPoint(Right, Top);

        public MpPoint BottomLeft => new MpPoint(Left, Bottom);
        public MpPoint BottomRight => new MpPoint(Right, Bottom);

        public double X {
            get => Points[0].X;
            set => Points[0].X = value;
        }
        public double Y {
            get => Points[0].Y;
            set => Points[0].Y = value;
        }

        public MpSize Size {
            get => new MpSize(Points[1].X, Points[1].Y);
            set => Points[1] = new MpPoint(value.Width, value.Height);
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
                if(value - X < 0) {
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

        #endregion

        #region Constructors


        public MpRect() : base() {
            Points = Enumerable.Repeat(new MpPoint(), 4).ToArray();
        }
        public MpRect(MpPoint location,MpSize size) {
            Points = new MpPoint[] { location, new MpPoint(size.Width, size.Height) };
        }

        public MpRect(double x, double y, double w, double h) : this(new MpPoint(x,y),new MpSize(w,h)) { }

        #endregion

        #region Public Methods
        public bool Contains(MpPoint p) {
            return p.X >= Left && p.X <= Right &&
                   p.Y >= Top && p.Y <= Bottom;
        }

        public bool Contains(MpRect other) {
            return Contains(other.TopLeft) && Contains(other.BottomRight);
        }
        public bool Intersects(MpRect other) {
            return other.Points.Any(x => Contains(x));
        }


        public void Union(MpRect b) {
            X = Math.Min(X, b.X);
            Width = Math.Max(X + Width, b.X + b.Width);
            Y = Math.Min(Y, b.Y);
            Height = Math.Max(Y + Height, b.Y + b.Height);
        }

        public override string ToString() {
            return $"X:{Location.X} Y:{Location.Y} Width: {Width} Height: {Height}";
        }
        #endregion

    }
}
