using MonkeyPaste.Common.Plugin;
using MonkeyPaste.Common;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace MonkeyPaste {
    public class MpQuillLoadRequestMessage : MpJsonObject {
        public string envName { get; set; } // will be wpf,android, etc.

        public bool isPasteRequest { get; set; } = false; //request should ONLY happen if encoded w/ templates

        public bool isConvertPlainHtmlRequest { get; set; }
        public bool isReadOnlyEnabled { get; set; } = true;

        public string itemEncodedHtmlData { get; set; }

        public List<MpTextTemplate> usedTextTemplates { get; set; }
    }

    public class MpQuillDisableReadOnlyRequestMessage : MpJsonObject {
        public List<MpTextTemplate> allAvailableTextTemplates { get; set; }
        public double editorHeight { get; set; }
    }

    public class MpQuillDisableReadOnlyResponseMessage : MpJsonObject {
        public double editorWidth { get; set; }
    }

    public class MpQuillEnableReadOnlyResponseMessage : MpJsonObject {
        public string itemEncodedHtmlData { get; set; }

        public List<string> userDeletedTemplateGuids { get; set; }
        public List<MpTextTemplate> updatedAllAvailableTextTemplates { get; set; }
    }
    
}
