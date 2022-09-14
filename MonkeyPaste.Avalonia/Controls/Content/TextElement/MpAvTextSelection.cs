using Avalonia.Controls;
using MonkeyPaste.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonkeyPaste.Avalonia {
    public class MpAvTextSelection : MpAvTextRange, MpAvITextSelection {
        private MpAvIContentDocument _document;

        //public MpAvTextPointer Start;

        #region Constructors

        public MpAvTextSelection(MpAvIContentDocument doc) : base(doc.ContentStart,doc.ContentStart) {
            _document = doc;
        }

        #endregion

        #region Public Methods

        public void Select(MpAvITextPointer start, MpAvITextPointer end) {
            if(_document is MpAvHtmlDocument htmlDoc &&
                htmlDoc.Owner is MpAvCefNetWebView wv) {
                var selSelReq = new MpQuillSetSelectionRangeRequestMessage() {
                    index = start.Offset,
                    length = end.Offset - start.Offset
                };

                wv.ExecuteJavascript($"setSelection_ext('{selSelReq.SerializeJsonObjectToBase64()}')");
            } else if(_document is MpAvTextBox tb) {
                tb.SelectionStart = start.Offset;
                tb.SelectionEnd = end.Offset;                
            }
        }

        #endregion
    }
}
