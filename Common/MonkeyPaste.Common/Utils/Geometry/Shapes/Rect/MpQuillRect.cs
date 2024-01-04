namespace MonkeyPaste.Common {
    public class MpJsonRect {
        //public double x { get; set; }
        //public double y { get; set; }
        public double left { get; set; }
        public double right { get; set; }
        public double bottom { get; set; }
        public double top { get; set; }
        public double width { get; set; }
        public double height { get; set; }

        public MpRect ToRect() {
            return new MpRect(left, top, width, height);
        }
    }
}
