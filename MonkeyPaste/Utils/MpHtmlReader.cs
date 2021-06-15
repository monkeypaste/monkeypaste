using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using Xamarin.Forms;

namespace MonkeyPaste {
    public class MpHtmlReader {
        private static readonly Lazy<MpHtmlReader> _Lazy = new Lazy<MpHtmlReader>(() => new MpHtmlReader());
        public static MpHtmlReader Instance { get { return _Lazy.Value; } }

        public MpHtmlTextTag ReadHtml(string html) {
            var docRoot = new MpHtmlTextTag() {
                TagType = MpHtmlTagType.DocumentRoot,
                Contents = html
            };

            var mc = Regex.Matches(
                html, 
                MpRegEx.Instance.GetRegExForTokenType(MpSubTextTokenType.HtmlTag), 
                RegexOptions.IgnoreCase | RegexOptions.ExplicitCapture | RegexOptions.Multiline);

            int curOffset = 0;
            string remainingHtml = html;
            var curParent = docRoot;
            foreach (Match m in mc) {
                foreach (Group mg in m.Groups) {
                    foreach (Capture c in mg.Captures) {                        
                        curOffset = html.IndexOf(c.Value);

                        if(IsTagOpen(c.Value)) {
                            var openCapture = c;
                            string tagName = GetHtmlTagName(c.Value);

                            int openOffset = curOffset;
                            int contentStartOffset = openOffset + tagName.Length + 2;

                            int closeOffset = -1;
                            int contentEndOffset = contentStartOffset;
                            Capture closeCapture = null;
                            //find closing tag
                            for (int i = c.Index+1; i < mg.Captures.Count; i++) {
                                var curTag = mg.Captures[i];
                                if(GetHtmlTagName(curTag.Value) == tagName) {
                                    closeCapture = curTag;
                                    closeOffset = html.IndexOf(curTag.Value);
                                    contentEndOffset = closeOffset - 1;
                                    break;
                                }
                            }
                            if(closeCapture == null) {
                                continue;
                            }
                            if(contentStartOffset > contentEndOffset) {
                                throw new Exception($"Offset off for tag {tagName} with start:{contentStartOffset} and end:{contentEndOffset}");
                            }

                            var tag = MpHtmlTextTag.Create(tagName, html.Substring(contentStartOffset, contentEndOffset),curOffset);

                            if(tag == null) {
                                continue;
                            }

                            curParent.Child = tag;
                            tag.Parent = curParent;
                            curParent = tag;
                        }
                    }
                }
            }
            MpConsole.WriteLine($"Parsed Html: \n{docRoot.ToString()}");
            return docRoot;
        }

        private string GetHtmlTagName(string tagText) {
            if(!IsHtmlTag(tagText)) {
                return string.Empty;
            }
            return tagText.Replace("<", string.Empty).Replace(">", string.Empty).ToLower();
        }

        private bool IsTagClose(string tagText) {
            if (!IsHtmlTag(tagText)) {
                return false;
            }
            return tagText.Contains(@"/>");
        }

        private bool IsTagOpen(string tagText) {
            if (!IsHtmlTag(tagText)) {
                return false;
            }
            return !IsTagClose(tagText);
        }

        private bool IsHtmlTag(string tagText) {
            if (tagText == null) {
                throw new Exception($"Not a valid html tag: {tagText}");
            }
            if (!tagText.Contains("<") || !tagText.Contains(">")) {
                throw new Exception($"Not a valid html tag: {tagText}");
            }
            var mc = Regex.Matches(
                tagText,
                MpRegEx.Instance.GetRegExForTokenType(MpSubTextTokenType.HtmlTag),
                RegexOptions.IgnoreCase | RegexOptions.ExplicitCapture | RegexOptions.Multiline);

            return mc.Count > 0;
        }

        private MpHtmlTextTag ReadHtmlTag(string html, int fromIdx = 0) {
            if (string.IsNullOrEmpty(html)) {
                return null;
            }

            //find first tags opening tag
            int soIdx = html.IndexOf("<");
            if(soIdx < 0) {
                //if no opening tag exists return whatever was passed as a None
                return new MpHtmlTextTag() { Contents = html };
            }

            int scIdx = html.IndexOf(">");
            if (scIdx < 0) {
                //if no opening tag exists return whatever was passed as a None
                return new MpHtmlTextTag() { Contents = html };
            }
            return null;
        }

        private string GetHtmlTagContent(string xml, string tag) {
            if (string.IsNullOrEmpty(xml) || string.IsNullOrEmpty(tag)) {
                return string.Empty;
            }
            tag = tag.Replace(@"<", string.Empty).Replace(@"/>", string.Empty);
            tag = @"<" + tag + @">";
            var strl = xml.Split(new string[] { tag }, StringSplitOptions.RemoveEmptyEntries).ToList();
            if (strl.Count > 1) {
                tag = tag.Replace(@"<", @"</");
                return strl[1].Substring(0, strl[1].IndexOf(tag));
            }
            return string.Empty;
            int sIdx = xml.IndexOf(tag);
            if (sIdx < 0) {
                return string.Empty;
            }
            sIdx += tag.Length;
            tag = tag.Replace(@"<", @"</");
            int eIdx = xml.IndexOf(tag);
            if (eIdx < 0) {
                return string.Empty;
            }
            return xml.Substring(sIdx, eIdx - sIdx);
        }

        public void Test1() {
            var assembly = IntrospectionExtensions.GetTypeInfo(typeof(MpClipDetailPageViewModel)).Assembly;
            var stream = assembly.GetManifestResourceStream("MonkeyPaste.Resources.Html.Editor.TestData.quillFormattedTextSample1.html");
            using (var reader = new System.IO.StreamReader(stream)) {
                var html = reader.ReadToEnd();

                ReadHtml(html);
            }
        }
    }
}
