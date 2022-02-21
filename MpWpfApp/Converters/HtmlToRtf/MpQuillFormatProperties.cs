using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using Windows.Networking;

namespace MpWpfApp {
    public class MpQuillFormatProperties {
        #region Singleton
        private static readonly Lazy<MpQuillFormatProperties> _Lazy = new Lazy<MpQuillFormatProperties>(() => new MpQuillFormatProperties());
        public static MpQuillFormatProperties Instance { get { return _Lazy.Value; } }
        #endregion

        public readonly string QuillHeader = "<!-- QuillFormat -->";

        public string[] QuillTagNames {
            get {
                return new string[] {
                    "#text",
                    "img",
                    "em",
                    "span",
                    "strong",
                    "u",
                    "br",
                    "a",
                    "p",
                    "li",
                    "ol"
                };
            }
        }

        public string[] QuillOpenTags {
            get {
                var tags = new List<string>();
                foreach(var t in QuillTagNames) {
                    tags.Add(string.Format(@"<{0}>", t));
                }
                return tags.ToArray();
            }
        }
    }
}
