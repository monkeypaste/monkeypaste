using System.Drawing;
using System;
using System.Linq;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.Diagnostics;

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
            var qr = MpJsonObject.DeserializeObject<MpQuillRect>(jsonStr);
            var r = new MpRect(qr.left,qr.top,qr.width,qr.height);
            return r;
        }
        #endregion

        #region Properties

        [JsonIgnore]
        public MpPoint Location {
            get => Points[0];
            //set => Points[0] = value;
        }

        [JsonIgnore]
        public MpSize Size {
            get => new MpSize(Right-Left,Bottom-Top);
            //get => new MpSize(Points[1].X, Points[1].Y);
            //set => Points[1] = new MpPoint(value.Width, value.Height);
        }

        [JsonIgnore]
        public MpPoint TopLeft => new MpPoint(Left, Top);
        [JsonIgnore]
        public MpPoint TopRight => new MpPoint(Right, Top);
        [JsonIgnore]
        public MpPoint BottomLeft => new MpPoint(Left, Bottom);
        [JsonIgnore]
        public MpPoint BottomRight => new MpPoint(Right, Bottom);

        [JsonProperty("x")]
        public double X {
            get => Points[0].X;
            set => Points[0].X = value;
        }
        [JsonProperty("y")]
        public double Y {
            get => Points[0].Y;
            set => Points[0].Y = value;
        }
        [JsonProperty("left")]
        public double Left {
            get => X;
            set => X = value;
        }
        [JsonProperty("top")]
        public double Top {
            get => Y;
            set => Y = value;
        }

        [JsonProperty("right")]
        public double Right {
            //get => X + Width;
            //set {
            //    if (value - X < 0) {
            //        //swap left & right
            //        double temp = X;
            //        X = value;
            //        Right = temp;
            //    } else {
            //        Width = value - X;
            //    }
            //}
            get => Points[1].X;
            set => Points[1].X = value;
        }
        [JsonProperty("bottom")]
        public double Bottom {
            //get => Y + Height;
            //set {
            //    if (value - Top < 0) {
            //        //swap left & right
            //        double temp = Y;
            //        Y = value;
            //        Bottom = temp;
            //    } else {
            //        Height = value - Y;
            //    }
            //}
            get => Points[1].Y;
            set => Points[1].Y = value;
        }

        [JsonProperty("width",Order = int.MaxValue - 2)]
        //[JsonIgnore]
        public double Width {
            get => Right - Left; //Size.Width;
            //set => Size.Width = value;
            //get => Right - Left;
            //set {
            //    if(Left + value < 0) {
            //        Debugger.Break();
            //    }
            //    Right = Math.Max(Left, Left + value);
            //}
        }

        [JsonProperty("height", Order = int.MaxValue - 1)]
        //[JsonIgnore]
        public double Height {
            get => Bottom - Top; //Size.Height;
            //set => Size.Height = value;
            //get => Bottom - Top;
            //set {
            //    if(Top +value < 0) {
            //        Debugger.Break();
            //    }
            //    Bottom = Math.Max(Top, Top + value);
            //}
        }

        #endregion

        #region Constructors


        public MpRect() : base() {
            Points = Enumerable.Repeat(new MpPoint(), 4).ToArray();
        }
        public MpRect(MpPoint location,MpSize size) {
            Points = new MpPoint[] { location, new MpPoint(location.X + size.Width, location.Y + size.Height) };
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
            Left = Math.Min(Left, b.Left);
            Top = Math.Min(Top, b.Top);
            Right = Math.Max(Right, b.Right);
            Bottom = Math.Max(Bottom, b.Bottom);
        }

        public override string ToString() {
            return $"X:{Location.X} Y:{Location.Y} Width: {Width} Height: {Height}";
        }
        #endregion

    }


    public class MpQuillRect {
        public double left { get; set; }
        public double right { get; set; }
        public double bottom { get; set; }
        public double top { get; set; }
        public double width { get; set; }
        public double height { get; set; }
    }
}
