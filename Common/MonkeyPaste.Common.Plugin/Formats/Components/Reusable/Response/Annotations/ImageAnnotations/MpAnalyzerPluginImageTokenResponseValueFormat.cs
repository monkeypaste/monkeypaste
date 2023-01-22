namespace MonkeyPaste.Common.Plugin {
    public class MpAnalyzerPluginImageTokenResponseValueFormat : MpAnnotationNodeFormat {
        public MpJsonPathProperty<double> x { get; set; } = new MpJsonPathProperty<double>(0);
        public MpJsonPathProperty<double> y { get; set; } = new MpJsonPathProperty<double>(0);
        public MpJsonPathProperty<double> width { get; set; } = new MpJsonPathProperty<double>(0);
        public MpJsonPathProperty<double> height { get; set; } = new MpJsonPathProperty<double>(0);


        public MpAnalyzerPluginImageTokenResponseValueFormat() { }
        public MpAnalyzerPluginImageTokenResponseValueFormat(double x, double y, double w, double h) {
            this.x = new MpJsonPathProperty<double>(x);
            this.y = new MpJsonPathProperty<double>(y);
            width = new MpJsonPathProperty<double>(w);
            height = new MpJsonPathProperty<double>(h);
        }

        public override string ToString() {
            if (x == null || y == null || width == null || height == null) {
                return base.ToString();
            }
            return string.Format(@"x:{0} y:{1} w:{2} h:{4}", x, y, width, height);
        }
    }

}
