using MonkeyPaste.Common.Plugin;
using System;

namespace MonkeyPaste.Common {
    public class MpTextRange :
        MpITextRange,
        MpIDocumentComponent,
        IComparable<MpTextRange> {
        #region Interfaces

        #region IComparable Implementation

        public int CompareTo(MpTextRange other) {
            // return -1 if this instance precedes obj
            // return 0 if this instance is obj
            // return 1 if this instance is after obj

            if (other == null) {
                return -1;
            }
            if (StartIdx < other.StartIdx) {
                return -1;
            }
            if (StartIdx > other.StartIdx) {
                return 1;
            }
            if (EndIdx < other.EndIdx) {
                return -1;
            }
            if (EndIdx > other.EndIdx) {
                return 1;
            }
            return 0;
        }
        #endregion
        #region MpIDocumentComponent Implementation
        public object Document { get; set; }

        public bool IsInSameDocument(MpIDocumentComponent other) {
            return Document == other.Document;
        }
        #endregion

        #region MpIDocumentTextRange Implementation
        int MpITextRange.Offset =>
            StartIdx;
        int MpITextRange.Length =>
            Count;

        #endregion
        #endregion

        #region Properties
        public int StartIdx { get; set; }
        public int Count { get; set; }
        public int EndIdx =>
            StartIdx + Count - 1;
        public int AfterEndIdx =>
            EndIdx + 1;
        public int BeforeStartIdx =>
            StartIdx - 1;
        #endregion

        #region Constructors
        public MpTextRange(object document) : this(document, 0, 0) { }
        public MpTextRange(object document, int[] vals) : this(document, vals[0], vals[1]) { }
        public MpTextRange(object document, int start, int count) {
            Document = document;
            StartIdx = start;
            Count = count;
        }
        #endregion

        #region Public Methods
        public bool Contains(int idx) {
            return idx >= StartIdx && idx <= EndIdx;
        }


        #endregion
    }
}
