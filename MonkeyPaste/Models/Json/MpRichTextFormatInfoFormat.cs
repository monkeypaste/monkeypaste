using MonkeyPaste.Common;
using Newtonsoft.Json;

namespace MonkeyPaste {
    public class MpRichTextFormatInfoFormat : MpJsonObject {
        public MpBlockTextFormatInfoFormat blockFormat { get; set; }
        public MpInlineTextFormatInfoFormat inlineFormat { get; set; }
        public MpEmbedTextFormatInfoFormat embedFormat { get; set; }
    }
    public class MpInlineTextFormatInfoFormat : MpJsonObject {
        public string background { get; set; } //hex(6)
        public string color { get; set; } //hex(6)
        public bool bold { get; set; }
        public bool italic { get; set; }
        public bool strike { get; set; }
        public bool underline { get; set; }
        public string font { get; set; }
        public double size { get; set; } //<double>px
        public string script { get; set; } //"super","sub"

        // not sure if these are needed
        public bool code { get; set; }
        public string link { get; set; } //uri
    }

    public class MpBlockTextFormatInfoFormat : MpJsonObject {
        public bool blockquote { get; set; }
        public int header { get; set; } // 1-N 
        public int indent { get; set; } // 1-N
        public string list { get; set; } // "bullet","ordered","check"
        public string align { get; set; } // "right","center","justify",""(left)
        public string direction { get; set; } // "rtl", "" (ltr)
        [JsonProperty("code-block")]
        public bool codeBlock { get; set; }
    }

    public class MpEmbedTextFormatInfoFormat : MpJsonObject {
        public string video { get; set; } //url
        public string image { get; set; } // img.src
        public string formula { get; set; } // katex string (see https://katex.org )
    }
}
