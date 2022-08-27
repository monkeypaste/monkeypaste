using AvaloniaEdit.Document;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonkeyPaste.Avalonia {
    public interface MpAvITextPointer : IComparable {
        object Document { get; }
        int Offset { get; }

        MpAvITextPointer DocumentStart { get; }
        MpAvITextPointer DocumentEnd { get; }

        MpAvITextPointer GetNextInsertionPosition(LogicalDirection dir);
        bool IsInSameDocument(MpAvITextPointer otp);
        int GetOffsetToPosition(MpAvITextPointer tp);
        MpAvITextPointer GetPositionAtOffset(int offset);
        MpAvITextPointer GetInsertionPosition(LogicalDirection dir);
    }
    public class MpAvTextPointer : MpAvITextPointer {
        #region Private Variables

        #endregion

        #region Properties
        public MpAvITextPointer DocumentStart {
            get {
                if (Document is MpAvHtmlDocument doc) {
                    return doc.ContentStart;
                }
                return null;
            }
        }

        public MpAvITextPointer DocumentEnd {
            get {
                if(Document is MpAvHtmlDocument doc) {
                    return doc.ContentEnd;
                }
                return null;
            }
        }
        public int Offset { get; set; }

        public object Document { get; private set; }

        #endregion

        #region Constructors

        public MpAvTextPointer(object document, int offset) {
            Document = document;
            Offset = offset;
        }

        #endregion

        #region Public Methods

        public int CompareTo(object obj) {
            if(obj is MpAvITextPointer otp) {
                if(!IsInSameDocument(otp)) {
                    return -1;
                }
                return Offset.CompareTo(otp.Offset);
            }
            return -1;
        }

        public bool IsInSameDocument(MpAvITextPointer otp) {
            return Document == otp.Document;
        }

        public MpAvITextPointer GetNextInsertionPosition(LogicalDirection dir) {
            if (Document is MpAvHtmlDocument doc) {
                if (dir == LogicalDirection.Forward) {
                    if (doc.ContentEnd.Offset == Offset) {
                        return null;
                    }
                    return new MpAvTextPointer(Document, Offset + 1);
                } else {
                    if(Offset == 0) {
                        return null;
                    }
                    return new MpAvTextPointer(Document, Offset - 1);
                }
            }
            return null;
        }

        public int GetOffsetToPosition(MpAvITextPointer tp) {
            if(!IsInSameDocument(tp)) {
                throw new Exception("Must be in same document");
            }
            return tp.Offset - Offset;
        }

        public MpAvITextPointer GetPositionAtOffset(int offset) {
            if (Document is MpAvHtmlDocument doc) {
                int newOffset = Offset + offset;
                if (newOffset < 0 || newOffset > doc.ContentEnd.Offset) {
                    throw new Exception($"Offset '{newOffset}' is out-of-bounds 0-'{doc.ContentEnd.Offset}')");
                }
                return new MpAvTextPointer(Document, newOffset);
            }
            return null;
        }

        public MpAvITextPointer GetInsertionPosition(LogicalDirection dir) {
            //if(Document is MpAvHtmlDocument doc) {
            //    doc.HtmlDocument.DocumentNode.
            //}
            return new MpAvTextPointer(Document, Offset + (dir == LogicalDirection.Forward ? 1 : -1));
        }

        #endregion

    }
}
