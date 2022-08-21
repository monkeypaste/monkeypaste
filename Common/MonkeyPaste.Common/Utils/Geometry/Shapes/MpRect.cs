using System.Linq;

namespace MonkeyPaste.Common {
    public class MpRect : MpShape {
        public static MpRect Empty => new MpRect();
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


        public MpRect() : base() {
            Points = Enumerable.Repeat(new MpPoint(), 4).ToArray();
        }
        public MpRect(MpPoint location,MpSize size) {
            Points = new MpPoint[] { location, new MpPoint(size.Width, size.Height) };
        }

        public MpRect(double x, double y, double w, double h) : this(new MpPoint(x,y),new MpSize(w,h)) { }

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

        public override string ToString() {
            return $"X:{Location.X} Y:{Location.Y} Width: {Width} Height: {Height}";
        }

    }
}
