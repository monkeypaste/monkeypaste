using System;
using System.ComponentModel;

namespace MonkeyPaste.Common {
    public class MpPoint : ICloneable { //, INotifyPropertyChanged {
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
        public static MpPoint operator *(MpPoint a, double val) {
            return new MpPoint(a.X * val, a.Y * val);
        }

        public static MpPoint operator /(MpPoint a, double val) {
            return new MpPoint(val == 0 ? 0 : a.X / val, val == 0 ? 0 : a.Y / val);
        }

        #endregion

        #endregion

        #region Properties
        public double X { get; set; } = 0;
        public double Y { get; set; } = 0;
        #endregion

        #region Events

        //public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        #region Constructors
        public MpPoint() { }
        public MpPoint(double x,double y) {
            X = x;
            Y = y;
        }
        #endregion


        #region Public Methods

        public double Distance(MpPoint other) {
            return Math.Sqrt(Math.Pow(other.X - X, 2) + Math.Pow(other.Y - Y, 2));
        }

        public double Length => Distance(MpPoint.Zero);

        public void Normalize() {
            double distance = Math.Sqrt(X * X + Y * Y);
            X /= distance;
            Y /= distance;
        }

        public void Clamp(MpRect rect) {
            Clamp(rect.Left, rect.Top, rect.Right, rect.Bottom);
        }
        public void Clamp(MpPoint min, MpPoint max) {
            Clamp(min.X, min.Y, max.X, max.Y);
        }

        public void Clamp(double min_x,double min_y,double max_x,double max_y) {
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
