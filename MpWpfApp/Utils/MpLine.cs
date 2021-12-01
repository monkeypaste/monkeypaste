using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace MpWpfApp {
    public class MpLine {
        public Point P1 { get; set; } = new Point();
        public Point P2 { get; set; } = new Point();

        public Point[] P => new Point[] { P1, P2 };

        public MpLine() { }

        public MpLine(Point p1, Point p2) {
            P1 = p1;
            P2 = p2;
        }

        public MpLine(double x1, double y1, double x2, double y2) {
            P1 = new Point(x1, y1);
            P2 = new Point(x2, y2);
        }

        public override string ToString() {
            return $"P1: ({P1.X},{P1.Y}),({P2.X},{P2.Y})";
        }
    }
}
