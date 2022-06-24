using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace MonkeyPaste.Common {
    public class MpLine : MpShape {
        public MpPoint P1 {
            get => Points[0];
            set => Points[0] = value;
        }
        public MpPoint P2 {
            get => Points[1];
            set => Points[1] = value;
        }

        

        public MpLine(MpPoint p1, MpPoint p2) {
            Points = new MpPoint[] { p1, p2 };
        }

        public MpLine() : this(new MpPoint(), new MpPoint()) { }

        public MpLine(double x1, double y1, double x2, double y2) 
            : this(new MpPoint(x1,y1),new MpPoint(x2,y2)) { }

        public override string ToString() {
            return $"P1: ({P1.X},{P1.Y}),({P2.X},{P2.Y})";
        }
    }
}
