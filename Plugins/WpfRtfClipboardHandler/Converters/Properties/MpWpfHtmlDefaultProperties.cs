using System.Collections.Generic;
namespace WpfRtfClipboardHandler {
    public static class MpWpfHtmlDefaultProperties {

        public static string[] QuillInlineTagNames {
            get {
                return new string[] {
                    "#text",
                    "span",
                    "a",
                    "em",
                    "strong",
                    "u",
                    "s",
                    "sub",
                    "sup",
                    "img"
                };
            }
        }

        public static string[] QuillBlockTagNames {
            get {
                return new string[] {
                    "p",
                    "ol",
                    "ul",
                    "li",
                    "div",
                    "table",
                    "colgroup",
                    "col",
                    "tbody",
                    "tr",
                    "td",
                    "iframe",
                    "blockquote"
                };
            }
        }

        public static string[] QuillTagNames {
            get {
                var allTags = new List<string>();
                allTags.AddRange(QuillBlockTagNames);
                allTags.AddRange(QuillInlineTagNames);
                return allTags.ToArray();
            }
        }

    }
}
