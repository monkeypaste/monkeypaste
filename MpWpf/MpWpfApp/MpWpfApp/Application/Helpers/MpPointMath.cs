using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MpWpfApp {
    public class MpPointMath {
        public static Point Add(Point a,Point b) {
            return new Point(a.X + b.X,a.Y + b.Y);
        }
        public static Point Subtract(Point a,Point b) {
            return new Point(a.X - b.X,a.Y - b.Y);
        }
        public static float Distance(Point a,Point b) {
            return (float)Math.Sqrt(Math.Pow(b.X - a.X,2) + Math.Pow(b.Y - a.Y,2));
        }
    }
}
