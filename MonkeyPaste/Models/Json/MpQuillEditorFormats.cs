using MonkeyPaste.Common.Plugin;
using MonkeyPaste.Common;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace MonkeyPaste {
    public class MpQuillInitMainRequestMessage : MpJsonObject {

        public string envName { get; set; } // will be wpf,android, etc.
    }

    public class MpQuillLoadContentRequestMessage : MpJsonObject {
        public string contentHandle { get; set; }
        public string contentType { get; set; }

        public string itemData { get; set; }

        public bool isPasteRequest { get; set; } = false; //request should ONLY happen if encoded w/ templates

        public string searchText { get; set; } = null;
        public bool isCaseSensitive { get; set; } = false;
        public bool isWholeWord { get; set; } = false;
        public bool useRegex { get; set; } = false;

        public bool isAppendLineMode { get; set; }
        public bool isAppendMode { get; set; }

        public string annotationsJsonStr { get; set; }
    }

    public class MpQuillContentDataRequestMessage : MpJsonObject {
        public List<string> formats { get; set; }

        public bool forOle { get; set; }
    }

    public class MpQuillContentScreenShotNotificationMessage : MpJsonObject {
        public string contentScreenShotBase64 { get; set; }
    }

    public class MpQuillContentFindReplaceVisibleChanedNotificationMessage : MpJsonObject {
        public bool isFindReplaceVisible { get; set; }
    }
    public class MpQuillContentQuerySearchRangesChangedNotificationMessage : MpJsonObject {
        public int rangeCount { get; set; }
    }

    public class MpQuillContentSearchRangeNavigationMessage : MpJsonObject {
        public int curIdxOffset { get; set; }
    }
    public class MpQuillDisableReadOnlyRequestMessage : MpJsonObject {
        // NOTE props ignored in Avalonia only for wpf...
        public List<MpTextTemplate> allAvailableTextTemplates { get; set; }
        public double editorHeight { get; set; }
    }

    public class MpQuillDisableReadOnlyResponseMessage : MpJsonObject {
        public double editorWidth { get; set; }
        public double editorHeight { get; set; }
    }

    public class MpQuillEditorContentChangedMessage : MpJsonObject {
        public string itemData { get; set; }

        public double editorWidth { get; set; }
        public double editorHeight { get; set; }
        public int length { get; set; }
        public int lines { get; set; }

        public bool hasTemplates { get; set; }
    }
    public class MpQuillUserDeletedTemplateNotification : MpJsonObject {
        public string userDeletedTemplateGuid { get; set; }
    }

    public class MpQuillTemplateAddOrUpdateNotification : MpJsonObject {
        public string addedOrUpdatedTextTemplateBase64JsonStr { get; set; }
    }

    public class MpQuillConvertPlainHtmlToQuillHtmlRequestMessage : MpJsonObject {
        public string data { get; set; }
        public string dataFormatType { get; set; }
        public bool isBase64 { get; set; }
    }

    public class MpQuillConvertPlainHtmlToQuillHtmlResponseMessage : MpJsonObject {
        public string quillHtml { get; set; }
        public string quillDelta { get; set; }
        public string sourceUrl { get; set; }
    }

    public class MpQuillIsHostFocusedChangedMessage : MpJsonObject {
        public bool isHostFocused { get; set; }
    }

    public class MpQuillSubSelectionChangedNotification : MpJsonObject {
        public bool isSubSelectionEnabled { get; set; }
    }



    public class MpQuillExceptionMessage : MpJsonObject {
        public string url { get; set; }
        public string msg { get; set; }

        public int lineNum { get; set; }
        public int colNum { get; set; }

        public string errorObjJsonStr { get; set; }
        public override string ToString() {
            return $"JS Exception. Line: {lineNum} Col: {colNum} Url:'{url}' Msg: '{msg}' ErrorObj: '{errorObjJsonStr.ToPrettyPrintJson()}'";
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
        public bool metaKey { get; set; }
        public bool altKey { get; set; }
        public bool shiftKey { get; set; }
        public bool escKey { get; set; }
    }
    public class MpQuillShowCustomColorPickerNotification : MpJsonObject {
        public string currentHexColor { get; set; }
        public string pickerTitle { get; set; }
    }
    public class MpQuillCustomColorResultMessage : MpJsonObject {
        public string customColorResult { get; set; }
    }

    public class MpQuillTemplateDbQueryRequestMessage : MpJsonObject {
        public List<string> templateTypes { get; set; }
    }

    public class MpQuillGetRequestNotification : MpJsonObject {
        public string requestGuid { get; set; }
        public string reqMsgFragmentJsonStr { get; set; } = String.Empty;
    }
    
    public class MpQuillGetResponseNotification : MpJsonObject {
        public string requestGuid { get; set; }
        public string responseFragmentJsonStr { get; set; } = string.Empty;
    }

    public class MpQuillNavigateUriRequestNotification : MpJsonObject {
        public string uri { get; set; }
        public List<string> modKeys { get; set; } 
    }

    public class MpQuillEditorSetClipboardRequestNotification : MpJsonObject {
        
    }
    public class MpQuillEditorDataTransferObjectRequestNotification : MpJsonObject {
        
    }
    public class MpQuillDataTransferCompletedNotification : MpJsonObject {
        public string dataTransferSourceUrl { get; set; }
        public string changeDeltaJsonStr { get; set; }
        public string sourceDataItemsJsonStr { get; set; }
    }

    public class MpQuillAppendStateChangedMessage : MpJsonObject {
        public bool isAppendLineMode { get; set; }
        public bool isAppendMode { get; set; }
        public bool isAppendManualMode { get; set; }

        public int appendDocIdx { get; set; }
        public int appendDocLength { get; set; }
        public string appendData { get; set; }

    }

    public class MpQuillSelectionChangedMessage : MpJsonObject {
        public int index { get; set; }
        public int length { get; set; }
    }

    public class MpQuillScrollChangedMessage : MpJsonObject {
        public int left { get; set; }
        public int top { get; set; }
    }

    public class MpQuillInternalContextIsVisibleChangedNotification : MpJsonObject {
        public bool isInternalContextMenuVisible { get; set; }
    }


    public class MpQuillDataTransferMessageFragment : MpJsonObject {
        
    }

    public class MpQuillHostDataItemFragment : MpJsonObject {
        public string format { get; set; }
        public string data { get; set; }
    }
    public class MpQuillContentDataResponseMessage : MpJsonObject {
        public List<MpQuillHostDataItemFragment> dataItems { get; set; }
        public bool isAllContent { get; set; }
    }

    public class MpQuillHostDataItemsMessageFragment : MpJsonObject {
        public List<MpQuillHostDataItemFragment> dataItems { get; set; }
        public string effectAllowed { get; set; }
        public string enc { get; set; } = "utf8";
    }
    public class MpQuillDragDropEventMessage : MpJsonObject {
        public bool ctrlKey { get; set; }
        public bool metaKey { get; set; }
        public bool altKey { get; set; }
        public bool shiftKey { get; set; }
        public bool escKey { get; set; }
        public string eventType { get; set; }
        public double screenX { get; set; }
        public double screenY { get; set; }
        public MpQuillHostDataItemsMessageFragment dataItemsFragment { get; set; }
    }


    //public class MpQuillDragEndMessage : MpJsonObject {
    //    public bool fromHost { get; set; } = true;
    //    public bool wasCancel { get; set; } = false;
    //    public MpQuillDataTransferMessageFragment dataTransfer { get; set; }
    //}
}
