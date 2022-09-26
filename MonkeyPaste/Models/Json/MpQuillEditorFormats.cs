using MonkeyPaste.Common.Plugin;
using MonkeyPaste.Common;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace MonkeyPaste {
    public class MpQuillInitMainRequestMessage : MpJsonObject {

        public string envName { get; set; } // will be wpf,android, etc.
        public bool isPlainHtmlConverter { get; set; }
        public bool useBetterTable { get; set; } = true;
    }

    public class MpQuillInitMainResponseMessage : MpJsonObject {

        public string mainStatus { get; set; }
    }

    public class MpQuillLoadContentRequestMessage : MpJsonObject {
        public string contentHandle { get; set; }
        public string contentType { get; set; }

        public string itemData { get; set; }

        public bool isPasteRequest { get; set; } = false; //request should ONLY happen if encoded w/ templates

       // public List<MpTextTemplate> usedTextTemplates { get; set; }
    }

    public class MpQuillLoadContentResponseMessage : MpJsonObject {
        public double contentWidth { get; set; }
        public double contentHeight { get; set; }
        //public int contentLength { get; set; }

        public bool hasTemplates { get; set; }

        //public List<string> decodedTemplateGuids { get; set; }
    }

    public class MpQuillContentDataRequestMessage : MpJsonObject {
        public string formatRequested { get; set; }

    }

    public class MpQuillContentDataResponseMessage : MpJsonObject {
        public string formattedContentData { get; set; }
    }



    public class MpQuillDisableReadOnlyRequestMessage : MpJsonObject {
        // NOTE props ignored in Avalonia only for wpf...
        public List<MpTextTemplate> allAvailableTextTemplates { get; set; }
        public double editorHeight { get; set; }
    }

    public class MpQuillIsAllSelectedResponseMessage : MpJsonObject {
       public bool isAllSelected { get; set; }
    }

    public class MpQuillDisableReadOnlyResponseMessage : MpJsonObject {
        public double editorWidth { get; set; }
    }

    public class MpQuillEnableReadOnlyResponseMessage : MpJsonObject {
        public string itemData { get; set; }

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

    public class MpQuillActiveTemplateGuidsRequestMessage : MpJsonObject {
        public List<string> templateGuids { get; set; }
    }

    public class MpQuillGetRangeTextRequestMessage : MpJsonObject {
        public int index { get; set; }
        public int length { get; set; }
    }

    public class MpQuillGetRangeTextResponseMessage : MpJsonObject {
        public string text { get; set; }
    }

    public class MpQuillGetRangeHtmlRequestMessage : MpJsonObject {
        public int index { get; set; }
        public int length { get; set; }
    }

    public class MpQuillGetRangeHtmlResponseMessage : MpJsonObject {
        public string html { get; set; }
    }

    public class MpQuillSetSelectionRangeRequestMessage : MpJsonObject {
        public int index { get; set; }
        public int length { get; set; }
    }
    public class MpQuillSetSelectionFromEditorPointMessage : MpJsonObject {
        public double x { get; set; }
        public double y { get; set; }
        public string state { get; set; }

        public string modkeyBase64Msg { get; set; }
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

    public class MpQuillGetEncodedRangeDataRequestMessage : MpJsonObject {
        public int index { get; set; }
        public int length { get; set; }
        public bool isPlainText { get; set; }
    }

    public class MpQuillGetEncodedRangeDataResponseMessage : MpJsonObject {
        public string encodedRangeData { get; set; }
    }

    public class MpQuillGetEditorScreenshotResponseMessage : MpJsonObject {
        public string base64ImgStr { get; set; }
    }

    public class MpQuillSubSelectionChangedNotification : MpJsonObject {
        public bool isSubSelectionEnabled { get; set; }
    }

    public class MpQuillDragDropDataObjectMessage : MpJsonObject {
        public string eventType { get; set; }
        public List<MpQuillDragDropDataObjectItemFragment> items { get; set; }
    }

    public class MpQuillDragDropDataObjectItemFragment : MpJsonObject {
        public string format { get; set; }
        public string data { get; set; }
    }

    public class MpQuillDragEndMessage : MpJsonObject {
        public bool fromHost { get; set; } = true;
        public MpQuillDataTransferMessageFragment dataTransfer { get; set; }
    }
    public class MpQuillDataTransferMessageFragment : MpJsonObject {
        public string dropEffect { get; set; }
    }
    public class MpQuillDragStartOrEndNotification : MpJsonObject {
        public bool isStart { get; set; }
    }

    public class MpQuillDropEffectChangedNotification : MpJsonObject {
        public string dropEffect { get; set; }
    }
    public class MpQuillEditorIndexFromPointRequestMessage : MpJsonObject {
        public double x { get; set; }
        public double y { get; set; }
        public bool snapToLine { get; set; } = true;

        public int fallBackIdx { get; set; } = -1;
    }

    public class MpQuillEditorIndexFromPointResponseMessage : MpJsonObject {
        public int docIdx { get; set; }
    }

    public class MpQuillContentSelectionChangedMessage : MpJsonObject {
        //public int copyItemId { get; set; }
        public int index { get; set; }
        public int length { get; set; }

        public string selText { get; set; }
        public bool isChangeBegin { get; set; }
    }

    public class MpQuillContentLengthChangedMessage : MpJsonObject {
        public int length { get; set; }
    }
    public class MpQuillContentDraggableChangedMessage : MpJsonObject {
        public bool isDraggable { get; set; }
    }


    public class MpQuillExceptionMessage : MpJsonObject {
        public string exType { get; set; }
        public string exData { get; set; }
        public override string ToString() {
            return $"Quill Exception ofType:'{exType}' withData: '{exData}'";
        }
    }
    public class MpQuillFileListDataFragment : MpJsonObject {
        public List<MpQuillFileListItemDataFragmentMessage> fileItems { get; set; }
    }
    public class MpQuillFileListItemDataFragmentMessage : MpJsonObject {
        public string filePath { get; set; }
        public string fileIconBase64 { get; set; }
    }


    public class MpQuillModifierKeysNotification : MpJsonObject {
        public bool ctrlKey { get; set; }
        public bool altKey { get; set; }
        public bool shiftKey { get; set; }
        public bool escKey { get; set; }
    }

    public class MpQuillIsHostDraggingMessage : MpJsonObject {
        public bool isDragging { get; set; }
    }
}
