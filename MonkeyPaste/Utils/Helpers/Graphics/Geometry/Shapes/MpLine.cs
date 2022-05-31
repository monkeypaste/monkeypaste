using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace MonkeyPaste {
    public class MpLine : MpShape {
        public MpPoint P1 { get; set; } = new MpPoint();
        public MpPoint P2 { get; set; } = new MpPoint();

        public MpPoint[] P => new MpPoint[] { P1, P2 };

        public override MpPoint[] Points => new MpPoint[] { P1, P2 };
        public MpLine() { }

        public MpLine(MpPoint p1, MpPoint p2) {
            P1 = p1;
            P2 = p2;
        }

        public MpLine(double x1, double y1, double x2, double y2) {
            P1 = new MpPoint(x1, y1);
            P2 = new MpPoint(x2, y2);
        }

        public override string ToString() {
            return $"P1: ({P1.X},{P1.Y}),({P2.X},{P2.Y})";
        }
    }
}
