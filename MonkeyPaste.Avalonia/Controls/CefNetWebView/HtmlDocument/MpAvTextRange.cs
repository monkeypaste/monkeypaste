using MonkeyPaste.Common;
using MonkeyPaste.Common.Avalonia;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonkeyPaste.Avalonia {
    public interface MpAvITextRange {
        MpAvITextPointer Start { get; }
        MpAvITextPointer End { get; }

        bool IsEmpty { get; }
        string Text { get; set; }

        bool IsPointInRange(MpPoint point);
    }
    public class MpAvTextRange : MpAvITextRange {
        #region Properties

        public MpAvITextPointer Start { get; }
        public MpAvITextPointer End { get; }
        public bool IsEmpty { get; }
        public string Text { 
            get {
                if(!Start.IsInSameDocument(End)) {
                    throw new Exception("Must be in same document");
                }
                if(Start.Document is MpAvHtmlDocument doc) {
                    int length = End.Offset - Start.Offset;
                    return doc.Html.Substring(Start.Offset, Math.Max(0, length)).ToPlainText();
                }
                return String.Empty;
            }
            set {
                if (!Start.IsInSameDocument(End)) {
                    throw new Exception("Must be in same document");
                }
                if (Start.Document is MpAvHtmlDocument doc) {
                    string html = doc.Html;
                    int length = End.Offset - Start.Offset;
                    html = html.Remove(Start.Offset, Math.Max(0, length));
                    doc.Html = html.Insert(Start.Offset, "<span>" + value + @"</span>");
                }
            }
        }

        #endregion

        #region Constructors

        public MpAvTextRange(MpAvITextPointer start, MpAvITextPointer end) {
            Start = start;
            End = end;
        }

        #endregion

        #region Public Methods

        public bool IsPointInRange(MpPoint point) {
            return false;
        }

        #endregion
    }

    public class MpAvTextSelection : MpAvTextRange {
        public MpAvTextSelection(MpAvITextPointer start, MpAvITextPointer end) : base(start,end) { }
    }
}
