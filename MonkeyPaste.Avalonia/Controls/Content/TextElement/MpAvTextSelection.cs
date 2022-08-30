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

        

        #region Constructors

        public MpAvTextSelection(MpAvIContentDocument doc) : base(doc.ContentStart,doc.ContentStart) {
            _document = doc;
        }

        #endregion

        #region Public Methods

        public void Select(MpAvITextPointer start, MpAvITextPointer end) {
            if(_document is MpAvHtmlDocument htmlDoc &&
                htmlDoc.Owner is MpAvCefNetWebView wv) {
                string selJsonStr = string.Format(@"{index:{0}, length:{1}}", start.Offset, end.Offset - start.Offset);
                wv.ExecuteJavascript($"setSelection('{selJsonStr}')");
            } else if(_document is MpAvTextBox tb) {
                tb.SelectionStart = start.Offset;
                tb.SelectionEnd = end.Offset;                
            }
        }

        #endregion
    }
}
