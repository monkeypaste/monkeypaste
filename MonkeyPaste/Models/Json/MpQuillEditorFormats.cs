using MonkeyPaste.Common;
using Org.BouncyCastle.Asn1.Crmf;
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace MonkeyPaste {
    public class MpQuillPostMessageResponse : MpJsonObject {
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public MpEditorBindingFunctionType msgType { get; set; }
        public string msgData { get; set; }
        public string handle { get; set; }
    }
    public class MpQuillInitMainRequestMessage : MpJsonObject {

        public string envName { get; set; } // will be wpf,android, etc.
        public MpQuillDefaultsRequestMessage defaults { get; set; }

        // fragment 'MpQuillAppendStateChangedMessage'
        public string appendStateFragment { get; set; }
        public bool isConverter { get; set; }
    }
    public class MpQuillDefaultsRequestMessage : MpJsonObject {
        public string defaultFontFamily { get; set; }
        public string defaultFontSize { get; set; }
        public bool isSpellCheckEnabled { get; set; }
        public string currentTheme { get; set; }
        public double bgOpacity { get; set; }

        public int maxUndo { get; set; }

        // fragment 'MpQuillEditorShortcutKeystringMessage'
        public string shortcutFragmentStr { get; set; }
    }


    public class MpQuillLoadContentRequestMessage : MpJsonObject {
        public int contentId { get; set; }
        public bool isSubSelectionEnabled { get; set; }
        // fragment 'MpQuillPasteButtonInfoMessage'
        public string pasteButtonInfoFragment { get; set; }
        public bool isReadOnly { get; set; }
        public string contentHandle { get; set; }
        public string contentType { get; set; }

        public string itemData { get; set; }

        // fragment 'MpQuillContentSearchesFragment'
        public string searchesFragment { get; set; }

        // fragment 'MpQuillAppendStateChangedMessage'
        public string appendStateFragment { get; set; }

        // TODO remove or isolate case for loading w/ annotations immediatly,
        // currently used in editor tester though
        public string annotationsJsonStr { get; set; }

        // fragment 'MpQuillEditorSelectionStateMessage'
        public string selectionFragment { get; set; }
    }

    public class MpQuillPasteButtonInfoMessage : MpJsonObject {
        public string pasteButtonIconBase64 { get; set; }
        public string pasteButtonTooltipText { get; set; }
    }

    public class MpQuillContentSearchesFragment : MpJsonObject {
        public List<MpQuillContentSearchRequestMessage> searches { get; set; }
    }
    public class MpQuillContentSearchRequestMessage : MpJsonObject {
        public string searchText { get; set; } = null;
        public bool isCaseSensitive { get; set; } = false;
        public bool isWholeWordMatch { get; set; } = false;
        public bool useRegEx { get; set; } = false;

        public bool isReplace { get; set; }
        public string replaceText { get; set; }

        public string matchType { get; set; }

    }

    public class MpQuillContentDataObjectRequestMessage : MpJsonObject {
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
        public bool isAbsoluteOffset { get; set; }
    }

    public class MpQuillDisableReadOnlyResponseMessage : MpJsonObject {
        public double editorWidth { get; set; }
        public double editorHeight { get; set; }
    }
    public class MpQuillScrollBarVisibilityChangedNotification : MpJsonObject {
        public bool isScrollBarXVisible { get; set; }
        public bool isScrollBarYVisible { get; set; }
    }

    public class MpQuillEditorContentChangedMessage : MpJsonObject {
        public string itemData { get; set; }

        public double contentHeight { get; set; } // NOTE ignoring width, it'll roughly be editorWidth unless wrapping is disabled like in dnd
        public double editorWidth { get; set; }
        public double editorHeight { get; set; }
        public int itemSize1 { get; set; } = -1;
        public int itemSize2 { get; set; } = -1;

        public bool hasTemplates { get; set; }
        public bool hasEditableTable { get; set; }

        public string dataTransferCompletedRespFragment { get; set; }
    }
    public class MpQuillUserDeletedTemplateNotification : MpJsonObject {
        public string userDeletedTemplateGuid { get; set; }
    }

    public class MpQuillUpdateContentRequestMessage : MpJsonObject {
        // fragment 'MpQuillDelta'
        public string deltaFragmentStr { get; set; }

        // fragment 'MpAnnotationNodeFormat'
        public string annotationFragmentStr { get; set; }
    }

    public class MpQuillLastTransactionUndoneNotification : MpJsonObject {
    }


    public class MpQuillConvertPlainHtmlToQuillHtmlRequestMessage : MpJsonObject {
        public string data { get; set; }
        public string dataFormatType { get; set; }
        public bool isBase64 { get; set; }
    }

    public class MpQuillConvertPlainHtmlToQuillHtmlResponseMessage : MpJsonObject {
        public string html { get; set; }
        public string quillHtml { get; set; }
        public string quillDelta { get; set; }
        public string sourceUrl { get; set; }

        public bool success { get; set; }
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

    public class MpQuillTemplateAddOrUpdateNotification : MpJsonObject {
        public string addedOrUpdatedTextTemplateBase64JsonStr { get; set; }
    }
    public class MpQuillTemplateDbQueryRequestMessage : MpJsonObject {
        public List<string> templateTypes { get; set; }
    }

    public class MpQuillShowDialogRequestMessage : MpJsonObject {
        public string title { get; set; }
        public string msg { get; set; }
        public string dialogType { get; set; }
        public string iconResourceObj { get; set; }
    }
    public class MpQuillShowDialogResponseMessage : MpJsonObject {
        public string dialogResponse { get; set; }
    }

    public class MpQuillSharedTemplateDataChangedMessage : MpJsonObject {
        // fragment 'MpTextTemplate'
        public string changedTemplateFragmentStr { get; set; }
        public string deletedTemplateGuid { get; set; }
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
        public string linkText { get; set; }
        public int linkDocIdx { get; set; }
        public string linkType { get; set; }
        public List<string> modKeys { get; set; }
    }

    public class MpQuillEditorSetClipboardRequestNotification : MpJsonObject {

    }
    public class MpQuillEditorClipboardDataObjectRequestNotification : MpJsonObject {

    }
    public class MpQuillEditorDragDataObjectRequestNotification : MpJsonObject {
        public string unprocessedDataItemsJsonStr { get; set; }
    }
    public class MpQuillDataTransferCompletedNotification : MpJsonObject {
        // fragment 'MpQuillDelta'
        public string changeDeltaJsonStr { get; set; }

        // fragment 'MpQuillHostDataItemsMessage'
        public string sourceDataItemsJsonStr { get; set; }
        // fragment 'MpQuillEditorContentChangedMessage'
        public string contentChangedMessageFragment { get; set; }
        public string transferLabel { get; set; }
    }

    public class MpQuillAppendStateChangedMessage : MpJsonObject {
        public bool isAppendLineMode { get; set; }
        public bool isAppendInsertMode { get; set; }
        public bool isAppendManualMode { get; set; }
        public bool isAppendPaused { get; set; }
        public bool isAppendPreMode { get; set; }

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
    public class MpQuillInternalContextMenuCanBeShownChangedNotification : MpJsonObject {
        public bool canInternalContextMenuBeShown { get; set; }
    }


    public class MpQuillDataTransferMessageFragment : MpJsonObject {

    }

    public class MpQuillHostDataItemFragment : MpJsonObject {
        public string format { get; set; }
        public string data { get; set; }
    }
    public class MpQuillContentDataObjectResponseMessage : MpJsonObject {
        public List<MpQuillHostDataItemFragment> dataItems { get; set; }
        public bool isAllContent { get; set; }
        public bool isNoneSelected { get; set; }
    }

    public class MpQuillHostDataItemsMessage : MpJsonObject {
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
        public MpQuillHostDataItemsMessage dataItemsFragment { get; set; }
    }

    public class MpQuillAnnotationSelectedMessage : MpJsonObject {
        public string annotationGuid { get; set; }
    }

    public class MpQuillShowDebuggerNotification : MpJsonObject {
        public string reason { get; set; }
    }

    public class MpQuillDragEndMessage : MpJsonObject {
        public bool fromHost { get; set; } = true;
        public bool wasCancel { get; set; } = false;
        //public MpQuillDataTransferMessageFragment dataTransfer { get; set; }
    }

    public class MpQuillEditorShortcutKeystringMessage : MpJsonObject {
        public List<MpQuillEditorShortcutKeystringItemFragment> shortcuts { get; set; }
    }

    public class MpQuillEditorShortcutKeystringItemFragment : MpJsonObject {
        public string shortcutType { get; set; }
        public string keys { get; set; }
    }
    public class MpQuillEditorSelectionStateMessage : MpJsonObject {
        public int index { get; set; }
        public int length { get; set; }

        public int scrollLeft { get; set; }
        public int scrollTop { get; set; }
    }
}
