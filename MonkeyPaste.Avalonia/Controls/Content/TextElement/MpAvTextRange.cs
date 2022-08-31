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

        //public string Text {
        //    get {
        //        if (!Start.IsInSameDocument(End)) {
        //            throw new Exception("Must be in same document");
        //        }
        //        if (Start.Document is MpAvHtmlDocument doc &&
        //            doc.Owner is MpAvCefNetWebView wv) {
        //            string text = wv.EvaluateJavascript($"getText()");
        //            return text;
        //        }
        //        return string.Empty;
        //    }
        //    set {
        //        if (!Start.IsInSameDocument(End)) {
        //            throw new Exception("Must be in same document");
        //        }
        //        if (Start.Document is MpAvHtmlDocument doc) {
        //            string html = doc.Html;
        //            int length = End.Offset - Start.Offset;
        //            html = html.Remove(Start.Offset, Math.Max(0, length));
        //            doc.Html = html.Insert(Start.Offset, "<span>" + value + @"</span>");
        //        }
        //    }
        //}

        #endregion

        #region Constructors

        public MpAvTextRange(MpAvITextPointer start, MpAvITextPointer end) {
            Start = start;
            End = end;
        }

        #endregion

        #region Public Methods

        public bool IsEmpty => Start.Offset == End.Offset;

        public async Task<string> GetTextAsync() {
            if (!Start.IsInSameDocument(End)) {
                throw new Exception("Must be in same document");
            }
            if (Start.Document is MpAvHtmlDocument doc &&
                doc.Owner is MpAvCefNetWebView wv) {
                var getRangeMsg = new MpQuillContentRangeMessage() {
                    index = Start.Offset,
                    length = End.Offset - Start.Offset
                };
                string text = await wv.EvaluateJavascriptAsync($"getText_ext('{getRangeMsg.Serialize()}')");
                return text;
            } else if(Start.Document is MpAvTextBox tb) {
                return tb.Text;
            }
            return string.Empty;
        }

        public async Task SetTextAsync(string text) {
            if (!Start.IsInSameDocument(End)) {
                throw new Exception("Must be in same document");
            }
            if (Start.Document is MpAvHtmlDocument doc &&
                doc.Owner is MpAvCefNetWebView wv) {
                // NOTE passing extra 'isHostJsonMsg' to ensure any clipboard text isn't confused as this json message
                var setRangeMsg = new MpQuillContentSetTextRangeMessage() {
                    index = Start.Offset,
                    length = End.Offset - Start.Offset,
                    text = text
                };
                await wv.EvaluateJavascriptAsync($"setTextInRange_ext('{setRangeMsg.Serialize()}')");
            } else if(Start.Document is MpAvTextBox tb) {
                tb.Text = tb.Text.ReplaceRange(Start.Offset, End.Offset - Start.Offset, text);                
            }
            End.Offset = Start.Offset + text.Length;
        }

        public async Task<bool> IsPointInRangeAsync(MpPoint point) {
            var srect = await Start.GetCharacterRectAsync(LogicalDirection.Forward);
            var erect = await End.GetCharacterRectAsync(LogicalDirection.Forward);
            srect.Union(erect);
            bool result = srect.Contains(point);
            MpConsole.WriteLine("IsPointInRange: ");
            MpConsole.WriteLine("range: " + this);
            MpConsole.WriteLine("rect: " + srect);
            MpConsole.WriteLine("point: " + point);
            MpConsole.WriteLine("result: " + (result ? "YES" : "NO"));
            return result;
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

        public override string ToString() {
            return $"Start: {Start.Offset} End: {End.Offset} Length: {(End.Offset - Start.Offset)}";
        }

        #endregion
    }
}
