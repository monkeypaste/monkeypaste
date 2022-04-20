using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using Windows.Networking;

namespace MpWpfApp {
    public static class MpQuillFormatProperties {

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
