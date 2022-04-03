using MonkeyPaste.Plugin;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace MonkeyPaste {
    public class MpQuillLoadRequestMessage : MpJsonObject {
        public string envName { get; set; } // will be wpf,android, etc.

        public bool isPasteRequest { get; set; } = false; //request should ONLY happen if encoded w/ templates

        public bool isReadOnlyEnabled { get; set; } = true;

        public string itemEncodedHtmlData { get; set; }

        public List<MpCopyItemOperation> itemOperations { get; set; }

        public List<MpTextTemplate> usedTextTemplates { get; set; }
    }

    public class MpQuillDisableReadOnlyRequestMessage : MpJsonObject {
        public List<MpTextTemplate> allAvailableTextTemplates { get; set; }
        public double editorHeight { get; set; }
    }

    public class MpQuillEnableReadOnlyResponseMessage : MpJsonObject {
        public string itemEncodedHtmlData { get; set; }

        public List<string> removedGuids { get; set; }
        public List<MpTextTemplate> updatedAllAvailableTextTemplates { get; set; }
    }

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
        public string script { get; set; } //"super","sub"
        public bool underline { get; set; }                                           
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
