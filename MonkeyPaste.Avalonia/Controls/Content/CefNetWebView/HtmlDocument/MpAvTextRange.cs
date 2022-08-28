using AvaloniaEdit.Document;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Avalonia;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonkeyPaste.Avalonia {

    public class MpAvTextRange : MpAvITextRange {
        #region Properties

        public MpAvITextPointer Start { get; set; }
        public MpAvITextPointer End { get; set; }
        public bool IsEmpty => Start == End;

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
            var srect = Start.GetCharacterRect(LogicalDirection.Forward);
            var erect = End.GetCharacterRect(LogicalDirection.Forward);
            srect.Union(erect);
            return srect.Contains(point);
        }

        public int CompareTo(object obj) {
            if (obj is MpAvITextRange otr) {
                if (!Start.IsInSameDocument(otr.Start)) {
                    throw new Exception("Cannot compare MpAvITextRange from another document");
                }
                int result = Start.CompareTo(otr.Start) + End.CompareTo(otr.End);
                return Math.Clamp(result, -1, 1);
            }

            throw new Exception("Can only be compared to another MpAvITextRange");
        }

        public bool Equals(MpAvITextRange other) {
            return CompareTo(other) == 0;
        }

        #endregion
    }
}
