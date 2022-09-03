using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace MonkeyPaste.Common {
    public class MpLine : MpShape {
        #region Properties

        public override MpPoint[] Points => new MpPoint[] { P1, P2 };
        public MpPoint P1 { get; set; }
        public MpPoint P2 { get; set; }

        #endregion


        #region Constructors


        public MpLine() : this(new MpPoint(), new MpPoint()) { }

        public MpLine(double x1, double y1, double x2, double y2) 
            : this(new MpPoint(x1,y1),new MpPoint(x2,y2)) { }

        public MpLine(MpPoint p1, MpPoint p2) {
            P1 = p1;
            P2 = p2;
        }

        #endregion

        public override string ToString() {
            return $"P1: ({P1.X},{P1.Y}),({P2.X},{P2.Y})";
        }
    }
}
