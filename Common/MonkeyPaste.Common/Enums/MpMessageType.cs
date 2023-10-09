namespace MonkeyPaste.Common {
    public enum MpMessageType {
        None,

        QueryCompleted,
        RequeryCompleted,
        QueryChanged,
        SubQueryChanged,
        JumpToIdxCompleted,
        InPlaceRequeryCompleted,
        TotalQueryCountChanged,

        ResizingMainWindowComplete,

        //UnexpandComplete,
        MainWindowOpening,
        MainWindowOpened,
        MainWindowClosing,
        MainWindowClosed,
        MainWindowInitialOpenComplete,

        AppWindowActivated,
        AppWindowDeactivated,

        MainWindowActivated,
        MainWindowDeactivated,

        MainWindowLocked,
        MainWindowUnlocked,

        MainWindowOrientationChangeBegin,
        MainWindowOrientationChangeEnd,

        MainWindowSizeChangeBegin,
        MainWindowSizeChanged,
        MainWindowSizeChangeEnd,

        MainWindowLoadComplete,

        StartupComplete,

        ShortcutAssignmentActivated,
        ShortcutAssignmentDeactivated,
        ShortcutRoutingProfileChanged,

        ItemInitialized,

        ItemDragBegin,
        ItemDragEnd,
        ItemDragCanceled,

        DropOverTraysBegin,
        DropOverTraysEnd,

        DropWidgetEnabledChanged,
        //ExternalDragBegin, 
        //ExternalDragEnd,

        ClipboardPresetsChanged,

        TrayScrollChanged,

        TraySelectionChanged,

        PreTrayLayoutChange,
        PostTrayLayoutChange,

        SidebarItemSizeChangeBegin,
        SidebarItemSizeChangeEnd,
        SidebarItemSizeChanged,

        SelectedSidebarItemChangeBegin,
        SelectedSidebarItemChangeEnd,

        PinTrayEmptyOrHasTile,

        PinTrayResizeBegin,
        PinTrayResizeEnd,
        PinTraySizeChanged,

        TrayZoomFactorChangeBegin,
        TrayZoomFactorChanged,
        TrayZoomFactorChangeEnd,

        ContentListScrollChanged, //has context (tile)
        ContentItemsChanged, //has context (tile)

        // START Sound ntf messages 

        ContentAdded,
        ContentPasted,

        AppError,

        AppendModeActivated,
        AppendModeDeactivated,
        AppendBufferChanged,

        RightClickPasteEnabled,
        RightClickPasteDisabled,

        AutoCopyEnabled,
        AutoCopyDisabled,

        // END Sound ntf messages 

        SettingsFilterTextChanged,

        ContentResized,
        ResizeContentCompleted,

        ChildWindowClosed,

        SettingsWindowOpened,

        AdvancedSearchExpandedChanged,

        FilterItemSizeChanged,

        DropWidgetOpened,

        TagSelectionChanged,

        SelectNextMatch,
        SelectPreviousMatch,

        SearchCriteriaItemsChanged,
        QuerySortChanged,

        TagTileNotificationAdded,
        TagTileNotificationRemoved,

        AccountDowngrade,
        AccountUpgrade,
        AccountInfoChanged,

        ThemeChanged,

        Loaded, //has context (object)
        Busy,
        NotBusy,
    }
}
