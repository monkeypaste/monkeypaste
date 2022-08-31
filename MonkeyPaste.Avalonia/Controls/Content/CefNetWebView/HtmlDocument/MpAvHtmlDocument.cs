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
            var pointMsg = new MpQuillEditorIndexFromPointMessage() {
                x = point.X,
                y = point.Y,
                snapToLine = true
            };
            string idxRespStr = await _owner.EvaluateJavascriptAsync($"getEditorIndexFromPoint_ext('{pointMsg.Serialize()}')");
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
            string html = await _owner.EvaluateJavascriptAsync("getHtml_ext()");
            return html;
        }

        private async Task SetHtmlAsync(string html) {
            await Task.Delay(1);
            if (_owner == null) {
                return;
            }
            _owner.ExecuteJavascript($"setHtml_ext('{html}')");
        }

        private async Task<string> GetTextAsync() {
            if (_owner == null) {
                return string.Empty;
            }
            string text = await _owner.EvaluateJavascriptAsync("getText_ext()");
            return text;
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
