namespace MonkeyPaste.Common {
    public class MpLine : MpShape {
        #region Statics

        public static MpLine Empty => new MpLine(0, 0, 0, 0);

        #endregion
        #region Properties

        public override MpPoint[] Points => new MpPoint[] { P1, P2 };
        public MpPoint P1 { get; set; }
        public MpPoint P2 { get; set; }

        #endregion


        #region Constructors


        public MpLine() : this(new MpPoint(), new MpPoint()) { }

        public MpLine(double x1, double y1, double x2, double y2)
            : this(new MpPoint(x1, y1), new MpPoint(x2, y2)) { }

        public MpLine(MpPoint p1, MpPoint p2) {
            P1 = p1;
            P2 = p2;
        }

        #endregion

        #region Public Methods

        public override string ToString() {
            return $"P1: ({P1.X},{P1.Y}), P2:({P2.X},{P2.Y})";
        }

        #endregion
    }
}
