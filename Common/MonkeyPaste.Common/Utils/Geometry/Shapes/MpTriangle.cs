namespace MonkeyPaste.Common {
    public class MpTriangle : MpShape {
        #region Properties
        public override MpPoint[] Points => new MpPoint[] { P1,P2,P3 };
        public MpPoint P1 { get; set; }
        public MpPoint P2 { get; set; }
        public MpPoint P3 { get; set; }
        #endregion

        #region Constructors
        public MpTriangle() {
            P1 = new MpPoint();
            P2 = new MpPoint();
            P3 = new MpPoint();
        }
        public MpTriangle(MpPoint p1, MpPoint p2, MpPoint p3) {
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
