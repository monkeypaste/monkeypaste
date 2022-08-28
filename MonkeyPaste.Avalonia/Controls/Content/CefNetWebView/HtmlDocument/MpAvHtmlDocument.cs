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

        IControl MpAvIContentDocument.Owner => _owner;
        public MpAvITextPointer ContentStart => new MpAvTextPointer(this, 0);
        public MpAvITextPointer ContentEnd => new MpAvTextPointer(this, Math.Max(0,Html.Length - 1));

        string MpAvIContentDocument.ContentData {
            get => Html;
            set => Html = value;
        }

        #endregion

        public string Html {
            get => GetHtml();
            set => SetHtml(value);
        }

        public HtmlDocument HtmlDocument {
            get {
                var htmlDoc = new HtmlDocument();
                htmlDoc.LoadHtml(Html);
                return htmlDoc;
            }
        }

        #endregion

        #region Constructors

        public MpAvHtmlDocument(MpAvCefNetWebView owner) {
            _owner = owner;
        }

        #endregion

        #region Public Methods

        public string GetHtml() {
            if(_owner == null) {
                return string.Empty;
            }
            string html = _owner.EvaluateJavascript("getHtml()");
            return html;
        }

        public void SetHtml(string html) {
            if(_owner == null) {
                return;
            }
            _owner.ExecuteJavascript($"setHtml('{html}')");
        }

        public MpAvITextPointer GetPosisitionFromPoint(MpPoint point, bool snapToText) {
            if (snapToText) {
                point.Clamp(_owner.Bounds.ToPortableRect());
            }
            string jsScriptStr = String.Format(@"getEditorIndexFromPoint({x: {0},y: {1}},true)", point.X, point.Y);
            string idxRespStr = _owner.EvaluateJavascript(jsScriptStr);
            if(int.TryParse(idxRespStr, out int offset)) {
                return new MpAvTextPointer(this, offset);
            }
            return null;
        }

        public IEnumerable<MpAvITextRange> FindAllText(string matchText, bool isCaseSensitive, bool matchWholeWord, bool useRegex) {
            string pattern = useRegex ? matchText : matchText.Replace(Environment.NewLine, string.Empty);           
            pattern = useRegex ? pattern : Regex.Escape(pattern);
            pattern = !useRegex && matchWholeWord ? $"\b{pattern}\b" : pattern;

            string input = HtmlDocument.Text;
            var mc = Regex.Matches(input, pattern, isCaseSensitive ? RegexOptions.None : RegexOptions.IgnoreCase);
            var matches = new List<MpAvITextRange>();
            
            foreach (Match m in mc) {
                foreach (Group mg in m.Groups) {
                    foreach (Capture c in mg.Captures) {
                        if(useRegex && matchWholeWord && !Regex.IsMatch(c.Value,$"\b{c.Value}\b")) {
                            continue;
                        }
                        matches.AddRange(FindText(c.Value));
                    }
                }
            }
            matches = matches.Distinct().ToList();

            return matches;
        }

        #endregion

        #region Private Methods
        
        private IEnumerable<MpAvITextRange> FindText(string matchText) {            
            var matches = new List<MpAvITextRange>();
            if (string.IsNullOrEmpty(matchText)) {
                return matches;
            }
            MpAvITextRange curMatch = null;
            int curMatchIdx = 0;
            var textNodes = HtmlDocument.DocumentNode.Descendants("#text");
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

        private IEnumerable<MpAvITextRange> FindText2(string matchText) {
            var matchNodeLookup = new List<KeyValuePair<HtmlNode,MpAvITextRange>>();
            if(string.IsNullOrEmpty(matchText)) {
                return new List<MpAvITextRange>();
            }

            HtmlNodeCollection curNodes = null;
            for (int i = 0; i < matchText.Length; i++) {
                char curMatchChar = matchText[i];
                if(i == 0) {
                    curNodes = FindNodes(curMatchChar.ToString());
                }

                var nodes_toAdd = new List<HtmlNode>();
                var nodes_toRemove = new List<HtmlNode>();
                foreach(var curNode in curNodes) {
                    MpAvITextRange last_match_range = null;
                    if(matchNodeLookup.Any(x=>x.Key == curNode)) {
                        last_match_range = matchNodeLookup.FirstOrDefault(x => x.Key == curNode).Value;
                    }
                    int lastMatchIdx = last_match_range == null ? -1 : last_match_range.End.Offset;
                    var match_range = FindTextHelper(curNode, curMatchChar,lastMatchIdx, out HtmlNode matchNode);

                    if(match_range == null) {
                        nodes_toRemove.Add(curNode);
                    } else {
                        if(matchNode == null) {
                            Debugger.Break();
                        }
                        if(!curNodes.Contains(matchNode)) {
                            nodes_toAdd.Add(matchNode);
                        }
                        if(last_match_range != null) {
                            var last_match_range_kvp = matchNodeLookup.FirstOrDefault(x => x.Key == curNode);
                            matchNodeLookup.Remove(last_match_range_kvp);
                        }
                        
                        var updated_match_range = new MpAvTextRange(
                            last_match_range == null ? match_range.Start : last_match_range.Start,
                            match_range.End);

                        matchNodeLookup.Add(new KeyValuePair<HtmlNode, MpAvITextRange>(matchNode, updated_match_range));
                    }
                }

                nodes_toRemove.ForEach(x => curNodes.Remove(x));
                nodes_toAdd.ForEach(x => curNodes.Add(x));
            }

            return matchNodeLookup.Select(x => x.Value).Distinct();
        }
        private MpAvITextRange FindTextHelper(HtmlNode fromNode, char matchChar, int lastMatchIdx, out HtmlNode matchNode) {
            if(fromNode == null) {
                matchNode = null;
                return null;
            }
            if(fromNode.NodeType == HtmlNodeType.Text) {
                int matchIdx = fromNode.InnerText.IndexOf(matchChar);
                if(matchIdx < 0) {
                    // match char not found
                    matchNode = null;
                    return null;
                }

                int s_idx = fromNode.InnerStartIndex + matchIdx;
                if(lastMatchIdx >= 0 && s_idx - lastMatchIdx > 1) {
                    // matched character is not first and not the next character from input text
                    // so reject the match
                    matchNode = null;
                    return null;
                }

                int e_idx = s_idx + 1;
                matchNode = fromNode;
                return new MpAvTextRange(
                         new MpAvTextPointer(this, s_idx),
                         new MpAvTextPointer(this, e_idx));
            }
            foreach(var cn in fromNode.ChildNodes) {
                // when descending into children and this is not first match idx substitute last idx to start of enclosing tag
                int childMatchIdx = lastMatchIdx < 0 ? -1 : cn.InnerStartIndex;
                var cn_match_range = FindTextHelper(cn, matchChar, childMatchIdx, out HtmlNode childMatchNode);
                if(cn_match_range == null) {
                    continue;
                }
                matchNode = childMatchNode;
                return cn_match_range;
            }
            // for traversing to next sibling, same thing as children
            // when this is not first match idx substitute last idx to start of enclosing tag
            int siblingMatchIdx = lastMatchIdx < 0 ? -1 : fromNode.NextSibling == null ? -1: fromNode.NextSibling.InnerStartIndex;
            var sn_match_range = FindTextHelper(fromNode.NextSibling, matchChar, siblingMatchIdx, out HtmlNode siblingMatchNode);
            matchNode = siblingMatchNode;
            return sn_match_range;
        }

        private HtmlNodeCollection FindNodes(string matchText) {
            return HtmlDocument.DocumentNode.SelectNodes($"//text()[contains(., '{matchText}')]");
        }

        #endregion
    }
}
