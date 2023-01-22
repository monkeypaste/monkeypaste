namespace MonkeyPaste.Common.Plugin {
    public class MpTextAnnotationFormat : MpAnnotationNodeFormat {
        public MpJsonPathProperty<int> rangeStart { get; set; }
        public MpJsonPathProperty<int> rangeLength { get; set; }


        public MpTextAnnotationFormat() { }
        public MpTextAnnotationFormat(int start, int end) {
            rangeStart = new MpJsonPathProperty<int>(start);
            rangeLength = new MpJsonPathProperty<int>(end);
        }
    }

}
