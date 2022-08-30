using Avalonia.Controls;
using Avalonia.Media;
using AvaloniaEdit.Document;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Avalonia;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonkeyPaste.Avalonia {

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
                if (Document is MpAvHtmlDocument doc) {
                    return doc.ContentEnd;
                }
                return null;
            }
        }
        public int Offset { get; set; }


        public MpAvIContentDocument Document { get; private set; }

        #endregion

        #region Constructors

        public MpAvTextPointer(MpAvIContentDocument document, int offset) {
            Document = document;
            Offset = offset;
        }

        #endregion

        #region Public Methods

        public int CompareTo(object obj) {
            if (obj is MpAvITextPointer otp) {
                if (!IsInSameDocument(otp)) {
                    throw new Exception("Cannot compare MpAvITextPointer from another document");
                }
                return Offset.CompareTo(otp.Offset);
            }
            throw new Exception("Can only be compared to another MpAvITextPointer");
        }

        public bool IsInSameDocument(MpAvITextPointer otp) {
            return Document == otp.Document;
        }

        public MpAvITextPointer GetNextInsertionPosition(LogicalDirection dir) {
            if (dir == LogicalDirection.Forward) {
                if (Document.ContentEnd.Offset == Offset) {
                    return null;
                }
                return new MpAvTextPointer(Document, Offset + 1);
            }
            if (Offset == 0) {
                return null;
            }
            return new MpAvTextPointer(Document, Offset - 1);
        }


        public MpAvITextPointer GetLineStartPosition(int lineOffset) {
            return null;
        }
        public MpAvITextPointer GetLineEndPosition(int lineOffset) {
            return null;
        }

        public int GetOffsetToPosition(MpAvITextPointer tp) {
            if (!IsInSameDocument(tp)) {
                throw new Exception("Must be in same document");
            }
            return tp.Offset - Offset;
        }

        public MpAvITextPointer GetPositionAtOffset(int offset) {
            int newOffset = Offset + offset;
            if (newOffset < 0 || newOffset > Document.ContentEnd.Offset) {
                throw new Exception($"Offset '{newOffset}' is out-of-bounds 0-'{Document.ContentEnd.Offset}')");
            }
            return new MpAvTextPointer(Document, newOffset);
        }

        public MpAvITextPointer GetInsertionPosition(LogicalDirection dir) {
            //if(Document is MpAvHtmlDocument doc) {
            //    doc.HtmlDocument.DocumentNode.
            //}
            return new MpAvTextPointer(Document, Offset + (dir == LogicalDirection.Forward ? 1 : -1));
        }

        public async Task<MpRect> GetCharacterRectAsync(LogicalDirection dir) {
            int offset = dir == LogicalDirection.Forward ? Offset : Math.Max(0, Offset - 1);

            if (Document.Owner is TextBox tb) {
                var ft = tb.ToFormattedText();
                var rect = ft.HitTestTextPosition(offset);
                return rect.ToPortableRect();
            } else if (Document.Owner is MpAvCefNetWebView wv) {
                string rectJsonStr = await wv.EvaluateJavascriptAsync($"getCharacterRect({offset})");
                return MpRect.ParseJson(rectJsonStr);
            }
            return MpRect.Empty;
        }

        #endregion

    }
}
