using System;

namespace MonkeyPaste.Common {
    public class MpPoint :
        ICloneable,
        MpIIsFuzzyValueEqual<MpPoint> { //, INotifyPropertyChanged {
        #region Statics
        public static MpPoint Zero => new MpPoint(0, 0);


        #region Operations
        public static MpPoint operator -(MpPoint a) {
            return new MpPoint(-a.X, -a.Y);
        }

        public static MpPoint operator +(MpPoint a, MpPoint b) {
            return new MpPoint(a.X + b.X, a.Y + b.Y);
        }

        public static MpPoint operator -(MpPoint a, MpPoint b) {
            return new MpPoint(a.X - b.X, a.Y - b.Y);
        }

        public static MpPoint operator *(MpPoint a, MpPoint b) {
            return new MpPoint(a.X * b.X, a.Y * b.Y);
        }

        public static MpPoint operator /(MpPoint a, MpPoint b) {
            return new MpPoint(b.X == 0 ? 0 : a.X / b.X, b.Y == 0 ? 0 : a.Y / b.Y);
        }
        public static MpPoint operator +(MpPoint a, double val) {
            return new MpPoint(a.X + val, a.Y + val);
        }
        public static MpPoint operator -(MpPoint a, double val) {
            return new MpPoint(a.X - val, a.Y - val);
        }
        public static MpPoint operator ^(MpPoint a, double val) {
            return new MpPoint(Math.Pow(a.X, val), Math.Pow(a.Y, val));
        }
        public static MpPoint operator *(MpPoint a, double val) {
            return new MpPoint(a.X * val, a.Y * val);
        }

        public static MpPoint operator /(MpPoint a, double val) {
            return new MpPoint(val == 0 ? 0 : a.X / val, val == 0 ? 0 : a.Y / val);
        }

        #endregion

        #endregion

        #region Interfaces

        #region MpIIsFuzzyValueEqual Implementation

        public bool IsValueEqual(MpPoint otherPoint, double thresh = 0) {
            if (otherPoint == null) {
                return false;
            }
            return Math.Abs(X - otherPoint.X) <= thresh &&
                    Math.Abs(Y - otherPoint.Y) <= thresh;
        }
        #endregion

        #endregion

        #region Properties

        public double[] Values => new double[] { X, Y };
        public double X { get; set; } = 0;
        public double Y { get; set; } = 0;
        #endregion

        #region Events

        //public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        #region Constructors
        public MpPoint() { }
        public MpPoint(double x, double y) {
            X = x;
            Y = y;
        }
        #endregion


        #region Public Methods

        public double Distance(MpPoint other) {
            return Math.Sqrt(Math.Pow(other.X - X, 2) + Math.Pow(other.Y - Y, 2));
        }


        public double Length => Distance(MpPoint.Zero);
        public MpPoint NormalizedUnitVector {
            get {
                var n = new MpPoint(X, Y);
                n.Normalize();
                if (n.X < 0) {
                    n.X = 1 + n.X;
                }
                if (n.Y < 0) {
                    n.Y = 1 + n.Y;
                }
                return n;
            }
        }
        public MpPoint Normalized {
            get {
                var n = new MpPoint(X, Y);
                n.Normalize();
                return n;
            }
        }
        public void Normalize() {
            double distance = Math.Sqrt(X * X + Y * Y);
            X /= distance;
            Y /= distance;
        }

        public double AngleBetween(MpPoint other) {
            // example :
            // two points (namely x1, y1, and x2, y2), I would like to calculate the angle between these two points,
            // presuming that when y1 == y2 and x1 > x2 the angle is 180 degrees
            // from https://stackoverflow.com/a/12892493/105028

            var diff = other - this;
            return Math.Atan2(diff.Y, diff.X) * 180.0 / Math.PI;
        }

        public void Clamp(MpRect rect) {
            Clamp(rect.Left, rect.Top, rect.Right, rect.Bottom);
        }
        public void Clamp(MpPoint min, MpPoint max) {
            Clamp(min.X, min.Y, max.X, max.Y);
        }

        public void Clamp(double min_x, double min_y, double max_x, double max_y) {
            X = MpMathExtensions.Clamp(X, min_x, max_x);
            Y = MpMathExtensions.Clamp(Y, min_y, max_y);
        }

        public override string ToString() {
            return string.Format(@"X: {0}, Y: {1}", X, Y);
        }

        public object Clone() {
            return new MpPoint(X, Y);
        }

        #endregion
    }
}
