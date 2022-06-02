namespace MonkeyPaste.Plugin {
    public class MpSize {
        public double Width { get; set; } = 0;
        public double Height { get; set; } = 0;

        public MpSize() { }
        public MpSize(double w, double h) {
            Width = w;
            Height = h;
        }
    }
}
