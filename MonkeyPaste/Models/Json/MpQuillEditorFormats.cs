using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace MonkeyPaste {
    public class MpQuillPostMessageResponse {
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public MpEditorBindingFunctionType msgType { get; set; }
        public string msgData { get; set; }
        public string handle { get; set; }
    }
    public class MpQuillInitMainResponseMessage {
        public string userAgent { get; set; }
    }
    public class MpQuillInitMainRequestMessage {

        public string envName { get; set; } // will be wpf,android, etc.
        public MpQuillDefaultsRequestMessage defaults { get; set; }

        // fragment 'MpQuillAppendStateChangedMessage'
        public string appendStateFragment { get; set; }
        public bool isConverter { get; set; }
    }
    public class MpQuillDefaultsRequestMessage {
        public int minLogLevel { get; set; }
        public bool isDebug { get; set; }
        public string defaultFontFamily { get; set; }
        public string defaultFontSize { get; set; }
        public bool isSpellCheckEnabled { get; set; }
        public string currentTheme { get; set; }
        public double bgOpacity { get; set; }

        public bool isRightToLeft { get; set; }
        public int maxUndo { get; set; }
        public bool isDataTransferDestFormattingEnabled { get; set; } = true;

        // fragment 'MpQuillEditorShortcutKeystringMessage'
        public string shortcutFragmentStr { get; set; }
    }


    public class MpQuillLoadContentRequestMessage {
        public double editorScale { get; set; } = MpCopyItem.DEFAULT_ZOOM_FACTOR;
        public bool breakBeforeLoad { get; set; }
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


    public class MpQuillContentSearchesFragment {
        public List<MpQuillContentSearchRequestMessage> searches { get; set; }
    }
    public class MpQuillContentSearchRequestMessage {
        public string searchText { get; set; } = null;
        public bool isCaseSensitive { get; set; } = false;
        public bool isWholeWordMatch { get; set; } = false;
        public bool useRegEx { get; set; } = false;

        public bool isReplace { get; set; }
        public string replaceText { get; set; }

        public string matchType { get; set; }

    }

    public class MpQuillContentDataObjectRequestMessage {
        public List<string> formats { get; set; }

        public bool selectionOnly { get; set; }
    }

    public class MpQuillContentScreenShotNotificationMessage {
        public string contentScreenShotBase64 { get; set; }
    }

    public class MpQuillContentFindReplaceVisibleChanedNotificationMessage {
        public bool isFindReplaceVisible { get; set; }
    }
    public class MpQuillContentQuerySearchRangesChangedNotificationMessage {
        public int rangeCount { get; set; }
    }

    public class MpQuillContentSearchRangeNavigationMessage {
        public int curIdxOffset { get; set; }
        public bool isAbsoluteOffset { get; set; }
    }

    public class MpQuillDisableReadOnlyResponseMessage {
        public double editorWidth { get; set; }
        public double editorHeight { get; set; }
    }
    public class MpQuillOverrideScrollNotification {
        public bool canScrollX { get; set; }
        public bool canScrollY { get; set; }
    }
    public class MpQuillShowToolTipNotification {
        public string tooltipHtml { get; set; }
        public string tooltipText { get; set; }
        public string gestureText { get; set; }
        public double anchorX { get; set; }
        public double anchorY { get; set; }
        public bool isVisible { get; set; }
    }

    public class MpQuillEditorContentChangedMessage {
        public string contentHandle { get; set; }
        public string itemPlainText { get; set; }
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
    public class MpQuillUserDeletedTemplateNotification {
        public string userDeletedTemplateGuid { get; set; }
    }

    public class MpQuillUpdateContentRequestMessage {
        // fragment 'MpQuillDelta'
        public string deltaFragmentStr { get; set; }

        // fragment 'MpAnnotationNodeFormat'
        public string annotationFragmentStr { get; set; }
    }

    public class MpQuillLastTransactionUndoneNotification {
    }


    public class MpQuillConvertPlainHtmlToQuillHtmlRequestMessage {
        public string data { get; set; }
        public string verifyText { get; set; }
        public string dataFormatType { get; set; }

        public bool isBase64 { get; set; }
    }

    public class MpQuillConvertPlainHtmlToQuillHtmlResponseMessage {
        public string html { get; set; }
        public string quillHtml { get; set; }
        public string quillDelta { get; set; }
        public string sourceUrl { get; set; }

        public bool success { get; set; }
    }

    public class MpQuillIsHostFocusedChangedMessage {
        public bool isHostFocused { get; set; }
    }

    public class MpQuillSubSelectionChangedNotification {
        public bool isSubSelectionEnabled { get; set; }
    }



    public class MpQuillExceptionMessage {
        public string label { get; set; }
        public string msg { get; set; }
        public override string ToString() {
            return $"JS Exception[{label}]: '{msg}' ";
        }
    }
    public class MpQuillFileListDataFragment {
        public List<MpQuillFileListItemDataFragmentMessage> fileItems { get; set; }
    }
    public class MpQuillFileListItemDataFragmentMessage {
        public string filePath { get; set; }
        public string fileIconBase64 { get; set; }
    }


    public class MpQuillModifierKeysNotification {
        public bool ctrlKey { get; set; }
        public bool metaKey { get; set; }
        public bool altKey { get; set; }
        public bool shiftKey { get; set; }
        public bool escKey { get; set; }
    }
    public class MpQuillShowCustomColorPickerNotification {
        public string currentHexColor { get; set; }
        public string pickerTitle { get; set; }
    }
    public class MpQuillCustomColorResultMessage {
        public string customColorResult { get; set; }
    }

    public class MpQuillTemplateAddOrUpdateNotification {
        public string addedOrUpdatedTextTemplateBase64JsonStr { get; set; }
    }
    public class MpQuillTemplateDbQueryRequestMessage {
        public List<string> templateTypes { get; set; }
    }

    public class MpQuillShowDialogRequestMessage {
        public string title { get; set; }
        public string msg { get; set; }
        public string dialogType { get; set; }
        public string iconResourceObj { get; set; }
    }
    public class MpQuillShowDialogResponseMessage {
        public string dialogResponse { get; set; }
    }

    public class MpQuillSharedTemplateDataChangedMessage {
        // fragment 'MpTextTemplate'
        public string changedTemplateFragmentStr { get; set; }
        public string deletedTemplateGuid { get; set; }
    }

    public class MpQuillGetRequestNotification {
        public string requestGuid { get; set; }
        public string reqMsgFragmentJsonStr { get; set; } = String.Empty;
    }

    public class MpQuillGetResponseNotification {
        public string requestGuid { get; set; }
        public string responseFragmentJsonStr { get; set; } = string.Empty;
    }

    public class MpQuillNavigateUriRequestNotification {
        public string uri { get; set; }
        public string linkText { get; set; }
        public int linkDocIdx { get; set; }
        public string linkType { get; set; }
        public bool needsConfirm { get; set; }
        public List<string> modKeys { get; set; }
    }

    public class MpQuillEditorSetClipboardRequestNotification {

    }
    public class MpQuillEditorClipboardDataObjectRequestNotification {

    }
    public class MpQuillEditorDragDataObjectRequestNotification {
        public string unprocessedDataItemsJsonStr { get; set; }
    }
    public class MpQuillDataTransferCompletedNotification {
        // fragment 'MpQuillDelta'
        public string changeDeltaJsonStr { get; set; }

        // fragment 'MpQuillHostDataItemsMessage'
        public string sourceDataItemsJsonStr { get; set; }
        // fragment 'MpQuillEditorContentChangedMessage'
        public string contentChangedMessageFragment { get; set; }
        public string transferLabel { get; set; }
    }

    public class MpQuillAppendStateChangedMessage {
        public bool isAppendLineMode { get; set; }
        public bool isAppendInsertMode { get; set; }
        public bool isAppendManualMode { get; set; }
        public bool isAppendPaused { get; set; }
        public bool isAppendPreMode { get; set; }

        public int appendDocIdx { get; set; }
        public int appendDocLength { get; set; }
        public string appendData { get; set; }


    }

    public class MpQuillSelectionChangedMessage {
        public int index { get; set; }
        public int length { get; set; }
    }

    public class MpQuillScrollChangedMessage {
        public int left { get; set; }
        public int top { get; set; }
    }

    public class MpQuillInternalContextIsVisibleChangedNotification {
        public bool isInternalContextMenuVisible { get; set; }
    }
    public class MpQuillInternalContextMenuCanBeShownChangedNotification {
        public bool canInternalContextMenuBeShown { get; set; }
    }


    public class MpQuillDataTransferMessageFragment {

    }

    public class MpQuillHostDataItemFragment {
        public string format { get; set; }
        public string data { get; set; }
    }
    public class MpQuillContentDataObjectResponseMessage {
        public List<MpQuillHostDataItemFragment> dataItems { get; set; }
        public bool isAllContent { get; set; }
        public bool isNoneSelected { get; set; }
    }

    public class MpQuillHostDataItemsMessage {
        public List<MpQuillHostDataItemFragment> dataItems { get; set; }
        public string effectAllowed { get; set; }
        public string enc { get; set; } = "utf8";
    }
    public class MpQuillDragDropEventMessage {
        public bool ctrlKey { get; set; }
        public bool metaKey { get; set; }
        public bool altKey { get; set; }
        public bool shiftKey { get; set; }
        public bool escKey { get; set; }
        public string eventType { get; set; }
        public double screenX { get; set; }
        public double screenY { get; set; }
        // fragment 'MpQuillHostDataItemsMessage'
        public string dataItemsFragment { get; set; }
    }

    public class MpQuillAnnotationSelectedMessage {
        public string annotationGuid { get; set; }
        public bool isDblClick { get; set; }
    }

    public class MpQuillShowDebuggerNotification {
        public string reason { get; set; }
    }

    public class MpQuillDragEndMessage {
        public bool fromHost { get; set; } = true;
        public bool wasCancel { get; set; } = false;
        //public MpQuillDataTransferMessageFragment dataTransfer { get; set; }
    }

    public class MpQuillEditorShortcutKeystringMessage {
        public List<MpQuillEditorShortcutKeystringItemFragment> shortcuts { get; set; }
    }

    public class MpQuillEditorShortcutKeystringItemFragment {
        public string shortcutType { get; set; }
        public string keys { get; set; }
    }
    public class MpQuillEditorSelectionStateMessage {
        public int index { get; set; }
        public int length { get; set; }

        public int scrollLeft { get; set; }
        public int scrollTop { get; set; }
    }

    public class MpQuillPasteButtonInfoMessage {
        public string pasteButtonIconBase64 { get; set; }
        public string pasteButtonTooltipText { get; set; }
        public string pasteButtonTooltipHtml { get; set; }
        public string infoId { get; set; }
        public bool isFormatDefault { get; set; } = true;
    }
    public class MpQuillPasteInfoFormatsClickedNotification {
        public string infoId { get; set; }
        public bool isExpanded { get; set; }
        public double offsetX { get; set; }
        public double offsetY { get; set; }
    }
    public class MpQuillEditorScaleChangedMessage {
        public double editorScale { get; set; } = 1.0d;
    }
}
