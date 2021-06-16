using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Xml;

namespace MonkeyPaste {
    public class MpHtmlTagReader {
        private static readonly Lazy<MpHtmlTagReader> _Lazy = new Lazy<MpHtmlTagReader>(() => new MpHtmlTagReader());
        public static MpHtmlTagReader Instance { get { return _Lazy.Value; } }

        public MpHtmlTextTagNode ReadTags(string html) {
            //create implicit document root
            var docRoot = new MpHtmlTextTagNode() {
                TagType = MpHtmlTagType.DocumentRoot,
                Contents = html
            };
            //contains yet-to-be closed tags where [0] will be added as child to docRoot
            var openTagLookup = new Dictionary<int,MpHtmlTextTagNode>();

            //create hierachial tag objects where each knows its contents and root offset
            string curBuffer = string.Empty;
            for (int i = 0; i < html.Length; i++) {
                curBuffer += html[i];
                int tagIdx = IndexOfHtmlTag(curBuffer);
                if(tagIdx >= 0) {
                    string curTagHtml = curBuffer.Substring(tagIdx);
                    string curTagName = GetHtmlTagName(curTagHtml);
                    if (IsTagOpen(curTagHtml)) {
                        var tag = MpHtmlTextTagNode.Create(curTagName, i - curTagHtml.Length + 1, html);
                        openTagLookup.Add(i, tag);
                        curBuffer = string.Empty;
                    } else if (IsTagClose(curTagHtml)) {
                        var openTagKvp = openTagLookup.Last();
                        openTagLookup.Remove(openTagKvp.Key);
                        var tag = openTagKvp.Value;

                        int tcsIdx = openTagKvp.Key + 1;
                        int tceIdx = i - curTagHtml.Length + 1;
                        tag.Contents = html.Substring(tcsIdx, tceIdx - tcsIdx);

                        if (tag == null) {
                            continue;
                        }

                        if (openTagLookup.Count > 0) {
                            tag.Parent = openTagLookup.Last().Value;
                            openTagLookup.Last().Value.AddChild(tag);
                        } else {
                            docRoot.AddChild(tag);
                            tag.Parent = docRoot;
                        }

                        curBuffer = string.Empty;
                    }
                }
            }


            return docRoot;
        }

        private string GetHtmlTagName(string tagText) {
            if(IsTagOpen(tagText)) {
                var tagName = tagText.Replace("<", string.Empty).Replace(">", string.Empty).ToLower().Trim();
                int spaceIdx = tagName.IndexOf(" ");
                if(spaceIdx >= 0) {
                    tagName = tagName.Substring(0, spaceIdx);
                }
                return tagName;
            }
            return tagText.Replace("</", string.Empty).Replace(">", string.Empty).ToLower().Trim();
        }

        private bool IsTagClose(string tagText) {
            return tagText.Contains(@"</");
        }

        private bool IsTagOpen(string tagText) {
            return !IsTagClose(tagText);
        }

        private int IndexOfHtmlTag(string tagText) {
            var m = Regex.Match(
                tagText,
                MpRegEx.Instance.GetRegExForTokenType(MpSubTextTokenType.HtmlTag),
                RegexOptions.IgnoreCase | RegexOptions.ExplicitCapture | RegexOptions.Multiline);

            if(m.Success) {
                return tagText.IndexOf(m.Value);
            }
            return -1;
        }

        private MpHtmlTextTagNode ReadHtmlTag(string html, int fromIdx = 0) {
            if (string.IsNullOrEmpty(html)) {
                return null;
            }

            //find first tags opening tag
            int soIdx = html.IndexOf("<");
            if(soIdx < 0) {
                //if no opening tag exists return whatever was passed as a None
                return new MpHtmlTextTagNode() { Contents = html };
            }

            int scIdx = html.IndexOf(">");
            if (scIdx < 0) {
                //if no opening tag exists return whatever was passed as a None
                return new MpHtmlTextTagNode() { Contents = html };
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
                var htmlDoc = new HtmlDocument();
                htmlDoc.LoadHtml(html);
                var htmlNodes = htmlDoc.DocumentNode.SelectNodes("//p");
                ReadTags(html);
            }
        }
    }
}
