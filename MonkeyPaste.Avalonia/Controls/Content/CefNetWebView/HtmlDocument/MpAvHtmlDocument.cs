using Avalonia.Controls;
using HtmlAgilityPack;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Avalonia;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MonkeyPaste.Avalonia {
    public class MpAvHtmlDocument : MpAvIContentDocument {
        #region Private Variables
        private MpAvCefNetWebView _owner;
        #endregion

        #region Properties

        #region MpAvIContentDocument Implmentation

        public IControl Owner => _owner;
        public MpAvITextPointer ContentStart { get; private set; }
        public MpAvITextPointer ContentEnd { get; private set; }

        public async Task<MpAvDataObject> GetDataObjectAsync(bool ignoreSelection, bool fillTemplates, bool isCutOrCopy, string[] formats = null) {
            if (Owner is MpAvCefNetWebView wv && 
                wv.DataContext is MpAvClipTileViewModel ctvm) {

                // clear screenshot
                ContentScreenShotBase64 = null;

                var contentDataReq = new MpQuillContentDataRequestMessage() { 
                    forPaste = ctvm.IsPasting,
                    forDragDrop = ctvm.IsTileDragging,
                    forCutOrCopy = isCutOrCopy
                };

                bool for_ole = contentDataReq.forPaste || contentDataReq.forDragDrop || contentDataReq.forCutOrCopy;

                bool ignore_ss = true;
                // NOTE when file is on clipboard pasting into tile removes all other formats besides file
                // and pseudo files are only needed for dnd comptaibility so its gewd
                bool ignore_pseudo_file = contentDataReq.forCutOrCopy;
                if (formats == null) {
                    // TODO need to implement disable preset stuff once clipboard ui is in use 
                    // for realtime RegisterFormats data
                    contentDataReq.formats = MpPortableDataFormats.RegisteredFormats.ToList();
                } else {
                    contentDataReq.formats = formats.ToList();
                }
                
                var contentDataRespStr = await wv.EvaluateJavascriptAsync($"contentRequest_ext('{contentDataReq.SerializeJsonObjectToBase64()}')");
                var contentDataResp = MpJsonObject.DeserializeBase64Object<MpQuillContentDataResponseMessage>(contentDataRespStr);

                if(contentDataResp.dataItems == null) {
                    return null;
                }
                var avdo = new MpAvDataObject();
                foreach(var di in contentDataResp.dataItems) {
                    avdo.SetData(di.format, di.data);
                }

                if (for_ole) {
                    if (ctvm.ItemType == MpCopyItemType.Image) {
                        avdo.SetData(MpPortableDataFormats.AvPNG, ctvm.CopyItemData.ToAvBitmap().ToByteArray());
                        //var bmp = ctvm.CopyItemData.ToAvBitmap();
                        //avdo.SetData(MpPortableDataFormats.Text, bmp.ToAsciiImage());
                        //avdo.SetData(MpPortableDataFormats.AvHtml_bytes, bmp.ToRichHtmlImage());
                        // TODO add colorized ascii maybe as html and rtf!!
                    } else if(!ignore_ss) {
                        // screen shot is async and js notifies w/ base64 property here
                        while (ContentScreenShotBase64 == null) { 
                            
                            await Task.Delay(100); 
                        }
                        avdo.SetData(MpPortableDataFormats.AvPNG, ContentScreenShotBase64);
                    }

                    if (ctvm.ItemType == MpCopyItemType.FileList) {
                        avdo.SetData(MpPortableDataFormats.AvFileNames, ctvm.CopyItemData);
                    } else if(!ignore_pseudo_file) {
                        // js doesn't set file stuff for non-files
                        avdo.SetData(
                            MpPortableDataFormats.AvFileNames,
                            ctvm.CopyItemData.ToFile(
                                forceNamePrefix: ctvm.CopyItemTitle,
                                forceExt: ctvm.ItemType == MpCopyItemType.Image ? "png" : "txt",
                                isTemporary: true));
                    }

                    bool add_tile_data = ctvm.ItemType != MpCopyItemType.Text ||
                                       (wv.IsAllSelected() || wv.Selection.Length == 0);
                    if (add_tile_data) {
                        avdo.SetData(MpPortableDataFormats.INTERNAL_CLIP_TILE_DATA_FORMAT, ctvm.PublicHandle);
                    }
                }

               
                avdo.MapAllPseudoFormats();

                //avdo.DataFormatLookup.Remove(MpPortableDataFormats.GetDataFormat(MpPortableDataFormats.AvPNG));
                //avdo.DataFormatLookup.Remove(MpPortableDataFormats.GetDataFormat(MpPortableDataFormats.INTERNAL_CLIP_TILE_DATA_FORMAT));
                //avdo.DataFormatLookup.Remove(MpPortableDataFormats.GetDataFormat(MpPortableDataFormats.AvFileNames));
                //avdo.DataFormatLookup.Remove(MpPortableDataFormats.GetDataFormat(MpPortableDataFormats.AvHtml_bytes));
                //avdo.DataFormatLookup.Remove(MpPortableDataFormats.GetDataFormat(MpPortableDataFormats.CefHtml));
                //avdo.DataFormatLookup.Remove(MpPortableDataFormats.GetDataFormat(MpPortableDataFormats.CefText));

                return avdo;
            }
            return null;
        }

        public string ContentScreenShotBase64 { get; set; }
        #endregion

        #endregion

        #region Constructors

        public MpAvHtmlDocument(MpAvCefNetWebView owner) {
            _owner = owner;
            ContentStart = new MpAvTextPointer(this, 0);
            ContentEnd = new MpAvTextPointer(this, 0);
        }

        #endregion

        #region Public Methods
        public void ProcessContentChangedMessage(MpQuillEditorContentChangedMessage contentChanged_ntf) {
            if(contentChanged_ntf == null) {
                // shouldn't be null
                Debugger.Break();
                return;
            }
            ContentEnd.Offset = contentChanged_ntf.length;
            if(_owner != null && _owner.BindingContext is MpAvClipTileViewModel ctvm) {
                if(contentChanged_ntf.length > 0) {
                    ctvm.CharCount = contentChanged_ntf.length;
                }
                if(contentChanged_ntf.lines > 0) {
                    ctvm.LineCount = contentChanged_ntf.lines;
                }
                if(contentChanged_ntf.itemData != null) {
                    ctvm.CopyItemData = contentChanged_ntf.itemData;
                }
                if(contentChanged_ntf.editorHeight > 0 && contentChanged_ntf.editorHeight > 0) {
                    ctvm.UnformattedContentSize = new MpSize(contentChanged_ntf.editorWidth, contentChanged_ntf.editorHeight);
                }
                

                if(_owner.IsContentLoaded) {
                    // trigger id change to reload item
                    ctvm.OnPropertyChanged(nameof(ctvm.CopyItemId));
                }
            }
        }

        public async Task<MpAvITextPointer> GetPosisitionFromPointAsync(MpPoint point, bool snapToText) {
            if (snapToText) {
                point.Clamp(_owner.Bounds.ToPortableRect());
            }
            var pointMsg = new MpQuillEditorIndexFromPointRequestMessage() {
                x = point.X,
                y = point.Y,
                snapToLine = true
            };
            string idxRespStr = await _owner.EvaluateJavascriptAsync($"getDocIdxFromPoint_ext('{pointMsg.SerializeJsonObjectToBase64()}')");
            if (int.TryParse(idxRespStr, out int offset)) {
                return new MpAvTextPointer(this, offset);
            }
            return null;
        }

        public async Task<IEnumerable<MpAvITextRange>> FindAllTextAsync(string matchText, bool isCaseSensitive, bool matchWholeWord, bool useRegex) {
            string pattern = useRegex ? matchText : matchText.Replace(Environment.NewLine, string.Empty);           
            pattern = useRegex ? pattern : Regex.Escape(pattern);
            pattern = !useRegex && matchWholeWord ? $"\b{pattern}\b" : pattern;

            string html = await GetHtmlAsync();
            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(html);

            string input = htmlDoc.Text;
            var mc = Regex.Matches(input, pattern, isCaseSensitive ? RegexOptions.None : RegexOptions.IgnoreCase);
            var matches = new List<MpAvITextRange>();
            
            foreach (Match m in mc) {
                foreach (Group mg in m.Groups) {
                    foreach (Capture c in mg.Captures) {
                        if(useRegex && matchWholeWord && !Regex.IsMatch(c.Value,$"\b{c.Value}\b")) {
                            continue;
                        }
                        matches.AddRange(FindText(htmlDoc.DocumentNode,c.Value));
                    }
                }
            }
            matches = matches.Distinct().ToList();

            return matches;
        }

        #endregion

        #region Private Methods
        private async Task<string> GetHtmlAsync() {
            if (_owner == null) {
                return string.Empty;
            }
            string htmlRespStr = await _owner.EvaluateJavascriptAsync("getHtml_ext()");
            var htmlRespObj = MpJsonObject.DeserializeBase64Object<MpQuillGetRangeHtmlResponseMessage>(htmlRespStr);
            return htmlRespObj.html;
        }


        private async Task<string> GetTextAsync() {
            if (_owner == null) {
                return string.Empty;
            }
            string getRangeResp = await _owner.EvaluateJavascriptAsync("getText_ext()");
            var rangeRespMsg = MpJsonObject.DeserializeBase64Object<MpQuillGetRangeTextResponseMessage>(getRangeResp);

            return rangeRespMsg.text;
        }

        private void SetText(string text) {
            if (_owner == null) {
                return;
            }
            _owner.ExecuteJavascript($"setText_ext('{text}')");
        }

        private IEnumerable<MpAvITextRange> FindText(HtmlNode docNode, string matchText) {            
            var matches = new List<MpAvITextRange>();
            if (string.IsNullOrEmpty(matchText)) {
                return matches;
            }
            MpAvITextRange curMatch = null;
            int curMatchIdx = 0;
            var textNodes = docNode.Descendants("#text");
            foreach(var tn in textNodes.OrderBy(x=>x.InnerStartIndex)) {
                for (int i = 0; i < tn.InnerText.Length; i++) {
                    if (tn.InnerText[i] == matchText[curMatchIdx]) {
                        int matchIdx = tn.InnerStartIndex + i;
                        curMatchIdx++;

                        if (curMatchIdx == 1) {
                            curMatch = new MpAvTextRange(new MpAvTextPointer(this, matchIdx), null);
                        }
                        if (curMatchIdx == matchText.Length) {
                            curMatch.End = new MpAvTextPointer(this, matchIdx);
                            matches.Add(curMatch);
                            curMatchIdx = 0;
                            curMatch = null;
                        }
                    } else {
                        curMatch = null;
                        curMatchIdx = 0;
                    }
                }
            }

            return matches;
        }


        #endregion
    }
}
