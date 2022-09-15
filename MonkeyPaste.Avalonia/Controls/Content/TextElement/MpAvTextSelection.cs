using Avalonia.Controls;
using GLib;
using MonkeyPaste.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonkeyPaste.Avalonia {
    public class MpAvTextSelection : MpAvTextRange, MpAvITextSelection {
        private MpAvIContentDocument _document;

        private string _text;
        public string Text {
            get {
                if (_document is MpAvTextBox tb) {
                    _text = tb.SelectedText;
                }
                return _text;
            }
            set {
                if(_text != value) {
                    _text = value;
                    if (_document is MpAvHtmlDocument htmlDoc &&
                        htmlDoc.Owner is MpAvCefNetWebView wv) {
                        var setTextMsg = new MpQuillContentSetTextRangeMessage() {
                            index = Start.Offset,
                            length = Length,
                            text = _text
                        };
                        wv.ExecuteJavascript($"setTextInRange_ext('{setTextMsg.SerializeJsonObjectToBase64()}')");
                        
                    } else if(_document is MpAvTextBox tb) {
                        tb.SelectedText = _text;
                    }
                    
                }
            }
        }

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

        public void UpdateSelectedTextFromEditor(string selText) {
            _text = selText;
        }
        #endregion
    }
}
