﻿using System;

namespace MonkeyPaste {
    public class MpPoint {
        public double X { get; set; } = 0;
        public double Y { get; set; } = 0;

        public MpPoint() { }
        public MpPoint(double x,double y) {
            X = x;
            Y = y;
        }

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

        public static MpPoint operator *(MpPoint a, double val) {
            return new MpPoint(a.X * val, a.Y * val);
        }

        public static MpPoint operator /(MpPoint a, double val) {
            return new MpPoint(val == 0 ? 0 : a.X / val, val == 0 ? 0 : a.Y / val);
        }

        public double Distance(MpPoint other) {
            return Math.Sqrt(Math.Pow(other.X - X, 2) + Math.Pow(other.Y - Y, 2));
        }

        public override string ToString() {
            return string.Format(@"X: {0}, Y: {1}", X, Y);
        }
    }
}