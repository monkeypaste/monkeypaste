using HtmlAgilityPack;
using MonkeyPaste.Common.Avalonia;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace MonkeyPaste.Avalonia {
    public class MpAvHtmlDocument  {
        #region Private Variables
        private MpAvCefNetWebView _owner;
        #endregion

        #region Properties
        public string Html { 
            get => GetHtml();
            set => SetHtml(value);
        }

        public MpAvTextPointer ContentStart => new MpAvTextPointer(this, 0);
        public MpAvTextPointer ContentEnd => new MpAvTextPointer(this, Math.Max(0,Html.Length - 1));


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

        public IEnumerable<MpAvITextRange> FindText(string input, bool isCaseSensitive, bool matchWholeWord, bool useRegEx) {
            input = input.Replace(Environment.NewLine, string.Empty);


            if (useRegEx) {
                string pattern = input;
                string pt = Html.ToPlainText();
                var mc = Regex.Matches(pt, pattern, isCaseSensitive ? RegexOptions.None : RegexOptions.IgnoreCase);

                var trl = new List<MpAvITextRange>();
                foreach (Match m in mc) {
                    foreach (Group mg in m.Groups) {
                        foreach (Capture c in mg.Captures) {
                            var c_trl = ContentStart.FindAllText(ContentEnd, c.Value);
                            trl.AddRange(c_trl);
                        }
                    }
                }
                trl = trl.Distinct().ToList();
                if (useRegEx && matchWholeWord) {
                    trl = trl.Where(x => Regex.IsMatch(x.Text, $"\b{x.Text}\b")).ToList();
                }
                return trl;
            }

            return ContentStart.FindAllText(ContentEnd, input, isCaseSensitive, matchWholeWord).ToList();
        }
        #endregion

    }
}
