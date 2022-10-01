using Avalonia.Controls;
using Google.Apis.PeopleService.v1.Data;
using Gtk;
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
using static System.Net.Mime.MediaTypeNames;

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

        public async Task<MpAvDataObject> GetDataObjectAsync(bool ignoreSelection, bool fillTemplates) {
            if (Owner is MpAvCefNetWebView wv && wv.DataContext is MpAvClipTileViewModel ctvm) {
                var contentDataReq = new MpQuillContentDataRequestMessage() { forPaste = true };
                contentDataReq.formats = await ctvm.GetSupportedDataFormatsAsync();
                var contentDataRespStr = await wv.EvaluateJavascriptAsync($"contentRequest_ext('{contentDataReq.SerializeJsonObjectToBase64()}')");
                var contentDataResp = MpJsonObject.DeserializeBase64Object<MpQuillContentDataResponseMessage>(contentDataRespStr);

                var avdo = new MpAvDataObject();
                foreach(var di in contentDataResp.dataItems) {
                    avdo.SetData(di.format, di.data);
                }
                avdo.MapAllPseudoFormats();
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
