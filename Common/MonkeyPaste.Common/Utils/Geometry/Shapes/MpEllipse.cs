namespace MonkeyPaste.Plugin {
    public class MpEllipse : MpShape {
        public MpPoint Center { get; set; } = new MpPoint();

        public MpSize Size { get; set; } = new MpSize();

        public MpEllipse() { }
        public MpEllipse(MpPoint center, MpSize size) {
            Center = center;
            Size = size;
        }
    }
}
