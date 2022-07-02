namespace MonkeyPaste.Common {
    public class MpRect : MpShape {
        public MpPoint Location {
            get => Points[0];
            set => Points[0] = value;
        }

        public MpSize Size {
            get => new MpSize(Points[1].X, Points[1].Y);
            set => Points[1] = new MpPoint(value.Width, value.Height);
        }

        public double Left => Location.X;
        public double Right => Location.X + Size.Width;

        public double Top => Location.Y;
        public double Bottom => Location.Y + Size.Height;

        public double Width {
            get => Size.Width;
            set => Size.Width = value;
        }

        public double Height {
            get => Size.Height;
            set => Size.Height = value;
        }


        public MpRect() : base() { }
        public MpRect(MpPoint location,MpSize size) {
            Points = new MpPoint[] { location, new MpPoint(size.Width, size.Height) };
        }

        public MpRect(double x, double y, double w, double h) : this(new MpPoint(x,y),new MpSize(w,h)) { }

        public bool Contains(MpPoint p) {
            return p.X >= Left && p.X <= Right &&
                   p.Y >= Top && p.Y <= Bottom;
        }
    }
}
