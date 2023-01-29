namespace MonkeyPaste.Common {
    public class MpTriangle : MpShape {
        #region Statics

        public static MpTriangle CreateEqualateralTriangle(MpPoint origin, double sideLength) {
            /*<Polygon Points="25,0 50,50 0,50" />*/

            double L = sideLength;
            double xo = origin.X;
            double yo = origin.Y;

            double x1 = L + xo;
            double y1 = yo;
            
            double x2 = L+L+xo;
            double y2 = L+L+yo;
            
            double x3 = xo;
            double y3 = L+L+yo;
            return new MpTriangle(x1,y1,x2,y2,x3,y3);

        }
        #endregion
        #region Properties
        public override MpPoint[] Points => new MpPoint[] { P1,P2,P3 };
        public MpPoint P1 { get; set; }
        public MpPoint P2 { get; set; }
        public MpPoint P3 { get; set; }
        #endregion

        #region Constructors
        public MpTriangle() : 
            this(new MpPoint(),new MpPoint(),new MpPoint()) { }

        public MpTriangle(double x1, double y1, double x2, double y2, double x3, double y3) :
            this(new MpPoint(x1,y1), new MpPoint(x2,y2), new MpPoint(x2,y3)) { }
        public MpTriangle(MpPoint p1, MpPoint p2, MpPoint p3) : base() {
            P1 = p1;
            P2 = p2;
            P3 = p3;
        }
        #endregion

        #region Public Methods

        public override string ToString() {
            return $"P1: {P1} P2: {P2} P3: {P3}";
        }


        #endregion
    }
}
