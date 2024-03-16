namespace MonkeyPaste {
    public enum MpEditorBindingFunctionType {
        // two-way (editor as source) *_get async requests
        getAllSharedTemplatesFromDb,
        getClipboardDataTransferObject,
        getDragDataTransferObject,
        getContactsFromFetcher,
        getMessageBoxResult,

        // two-way (host as source) *_ext_ntf requests
        notifySelectionState,
        notifyPlainHtmlConverted,
        notifyReadOnlyEnabledFromHost,
        notifyDataObjectResponse,


        // one-way *_ntf notifications
        notifyShowToolTip,
        notifyDocSelectionChanged,
        notifyContentChanged,
        notifySubSelectionEnabledChanged,
        notifyException,
        notifyReadOnlyEnabled,
        notifyReadOnlyDisabled,
        notifyInitComplete,
        notifyDomLoaded,
        notifyDropCompleted,
        notifyDragEnter,
        notifyDragLeave,
        notifyDragStart,
        notifyDragEnd,
        notifyContentScreenShot,
        notifyUserDeletedTemplate,
        notifyAddOrUpdateTemplate,
        notifyPasteRequest,
        notifyFindReplaceVisibleChange,
        notifyQuerySearchRangesChanged,
        notifyLoadComplete,
        notifyShowCustomColorPicker,
        notifyNavigateUriRequested,
        notifySetClipboardRequested,
        notifyDataTransferCompleted,
        notifyAppendStateChanged,
        notifyInternalContextMenuIsVisibleChanged,
        notifyInternalContextMenuCanBeShownChanged,
        notifyLastTransactionUndone,
        notifyAnnotationSelected,
        notifyShowDebugger,
        notifyScrollBarVisibilityChanged,
        notifyPasteInfoFormatsClicked,
        notifyAppendStateChangeComplete,
        notifyPointerEvent,
        notifyContentImageLoaded
    }
}
