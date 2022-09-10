using MonkeyPaste.Common.Plugin;
using MonkeyPaste.Common;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace MonkeyPaste {
    public class MpQuillLoadRequestMessage : MpJsonObject {
        public int copyItemId { get; set; }
        public string envName { get; set; } // will be wpf,android, etc.

        public bool isPasteRequest { get; set; } = false; //request should ONLY happen if encoded w/ templates

        public bool isEditorPlainHtmlConverter { get; set; }
        public bool isReadOnlyEnabled { get; set; } = true;

        public string itemEncodedHtmlData { get; set; }

        public List<MpTextTemplate> usedTextTemplates { get; set; }
    }

    public class MpQuillLoadResponseMessage : MpJsonObject {
        public double contentWidth { get; set; }
        public double contentHeight { get; set; }

        public List<string> decodedTemplateGuids { get; set; }
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

    public class MpQuillTextTemplateBlot : MpJsonObject {
        /*
        isFocus: "false"
templateColor: "#6F562C"
templateData: ""
templateDeltaFormat: "[object Object]"
templateGuid: "1cd87443-ee3c-46bb-b4f1-925205fc0f55"
templateHtmlFormat: "Template #1"
templateInstanceGuid: "21d2ea4d-64b0-491e-8552-8cee6c8af75e"
templateName: "Template #1"
templateText: "undefined"
templateType: "dynamic"
        */
        public bool isFocus { get; set; }
        public string templateColor { get; set; }
        public string templateData { get; set; }
        public string templateDeltaFormat { get; set; }
        public string templateGuid { get; set; }
        public string templateInstanceGuid { get; set; }
        public string templateName { get; set; }
        public string templateText { get; set; }
        public string templateType { get; set; }
    }

    public class MpQuillContentRangeMessage : MpJsonObject {
        public int index { get; set; }
        public int length { get; set; }
    }

    public class MpQuillConvertPlainHtmlToQuillHtmlRequestMessage : MpJsonObject {
        public string plainHtml { get; set; }
    }

    public class MpQuillConvertPlainHtmlToQuillHtmlResponseMessage : MpJsonObject {
        public string quillHtml { get; set; }
    }

    public class MpQuillContentSetTextRangeMessage : MpJsonObject {
        public int index { get; set; }
        public int length { get; set; }

        public string text { get; set; }

        public bool isHostJsonMsg => true;
    }
    

    public class MpQuillEditorIndexFromPointMessage : MpJsonObject {
        public double x { get; set; }
        public double y { get; set; }
        public bool snapToLine { get; set; } = true;

        public int fallBackIdx { get; set; } = -1;
    }

    public abstract class MpQuillContentMessageBase : MpJsonObject {
        public int copyItemId { get; set; }
    }
    public class MpQuillContentSelectionChangedMessage : MpQuillContentMessageBase {
        //public int copyItemId { get; set; }
        public int index { get; set; }
        public int length { get; set; }

        public List<MpJsonRect> selRects { get; set; }
        public string selJsonRectListBase64Str { get; set; }
    }

    public class MpQuillContentLengthChangedMessage : MpQuillContentMessageBase {
        // public int copyItemId { get; set; }
        public int length { get; set; }
    }
    public class MpQuillContentDraggableChangedMessage : MpQuillContentMessageBase {
        //public int copyItemId { get; set; }
        public bool isDraggable { get; set; }
    }
}
