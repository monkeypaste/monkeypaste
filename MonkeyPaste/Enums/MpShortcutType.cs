namespace MonkeyPaste {
    public enum MpShortcutType {
        None = 0,
        // GLOBALS
        ShowMainWindow,
        ToggleAppendMode,
        ToggleAppendLineMode,
        ToggleAutoCopyMode,
        ToggleRightClickPasteMode,
        ToggleListenToClipboard,

        // APPLICATION
        HideMainWindow,
        ExitApplication,
        PasteSelectedItems,
        PasteHere,
        CopySelection,
        CutSelection,
        DeleteSelectedItems,
        SelectNextColumnItem,
        SelectPreviousColumnItem,
        SelectNextRowItem,
        SelectPreviousRowItem,
        AssignShortcut,
        ChangeColor,
        Undo,
        Redo,
        EditContent,
        EditTitle,
        Duplicate,
        ScrollToHome,
        ScrollToEnd,
        WindowSizeUp,
        WindowSizeDown,
        WindowSizeLeft,
        WindowSizeRight,
        PreviousPage,
        NextPage,
        FindAndReplaceSelectedItem,
        ToggleMainWindowLocked,
        ToggleFilterMenuVisible,
        ShowSettings,
        // CUT-OFF 
        MAX_APP_SHORTCUT,
        //remaining are data (not context) driven using commandId
        PasteCopyItem = 101,
        SelectTag = 102,
        AnalyzeCopyItemWithPreset = 103,
        TriggerAction = 104
    }
    
}
