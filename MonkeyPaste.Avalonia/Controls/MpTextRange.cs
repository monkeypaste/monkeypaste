namespace MonkeyPaste.Avalonia {
    public class MpTextRange {
        public object Document { get; set; }
        public int StartIdx { get; set; }
        public int Count { get; set; }
        public int EndIdx =>
            StartIdx + Count - 1;
        public int AfterEndIdx =>
            EndIdx + 1;
        public int BeforeStartIdx =>
            StartIdx - 1;

        public MpTextRange() : this(0, 0) { }
        public MpTextRange(int[] vals) : this(vals[0], vals[1]) { }
        public MpTextRange(int start, int count) {
            StartIdx = start;
            Count = count;
        }

        public bool IsInSameDocument(MpTextRange other) {
            return Document == other.Document;
        }
        public bool Contains(int idx) {
            return idx >= StartIdx && idx <= EndIdx;
        }
    }
}
