namespace MonkeyPaste.Common {
    public class MpSize {
        //public static MpSize Parse(string text) {
        //    text = text.Trim();
        //    if(text.Contains())
        //}
        public double Width { get; set; } = 0;
        public double Height { get; set; } = 0;

        public MpSize() { }
        public MpSize(double w, double h) {
            Width = w;
            Height = h;
        }
    }
}
