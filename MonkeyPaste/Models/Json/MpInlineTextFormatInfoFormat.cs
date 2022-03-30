using MonkeyPaste.Plugin;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace MonkeyPaste {
    public class MpInlineTextFormatInfoFormat : MpJsonObject {
        public string background { get; set; } //hex(6)
        public bool bold { get; set; }
        public string color { get; set; } //hex(6)
        public string font { get; set; }
        public bool code { get; set; }
        public bool italic { get; set; }
        public string link { get; set; } //uri
        public string size { get; set; } //<double>px
        public bool strike { get; set; }
        public string script { get; set; } //'super','sub'
        public bool underline { get; set; }                                           
    }

    public class MpBlockTextFormatInfoFormat : MpJsonObject {
        public bool blockquote { get; set; }
        public int header { get; set; } // 1-N 
        public int indent { get; set; } // 1-N
        public string list { get; set; } // 'bullet','ordered','check'
        public string align { get; set; } // 'right','center','justify',''(left)
        public string direction { get; set; } // 'rtl', '' (ltr)
        [JsonProperty("code-block")]
        public bool codeBlock { get; set; }
    }

    public class MpEmbedTextFormatInfoFormat : MpJsonObject {
        public string video { get; set; } //url
        public string image { get; set; } // img.src
        public string formula { get; set; } // katex string (see https://katex.org )
    }

    public class MpTextTemplateFormat : MpJsonObject {
        public string templateGuid { get; set; }

        public string templateType { get; set; }
        public string templateData { get; set; }

        public string templateName { get; set; }
        public string templateColor { get; set; }

        public 
    }
}
