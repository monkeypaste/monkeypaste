namespace MonkeyPaste.Common {
    public class MpEllipse : MpShape {
        public override MpPoint[] Points => new MpPoint[] { Center };
        public MpPoint Center { get; set; } = new MpPoint();

        public MpSize Size { get; set; } = new MpSize();

        public MpEllipse() { }
        public MpEllipse(MpPoint center, MpSize size) {
            Center = center;
            Size = size;
        }
    }
}
