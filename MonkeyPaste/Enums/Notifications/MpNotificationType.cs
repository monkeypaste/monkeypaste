namespace MonkeyPaste {
    public enum MpNotificationType {
        None = 0,
        // Loader
        Loader,

        // Message
        DbError,
        Help,
        PluginUpdated,
        Message,
        AppModeChange,
        TrialExpired,

        ContentCapReached,
        TrashCapReached,
        ContentAddBlockedByAccount,
        ContentRestoreBlockedByAccount,

        AppendModeChanged,
        TriggerEnabled,
        AlertAction,
        StartupComplete,
        FileIoWarning,

        // User Action (System Tray)
        InvalidPlugin,
        InvalidClipboardFormatHandler,
        InvalidAction,
        BadHttpRequest,
        AnalyzerTimeout,
        InvalidRequest,
        InvalidResponse,
        ExecuteParametersRequest,

        // User Action (Modal) 

        ModalContentFormatDegradation,
        ModalYesNoCancelMessageBox,
        ModalYesNoMessageBox,
        ModalOkCancelMessageBox,
        ModalOkMessageBox,
        ModalTextBoxOkCancelMessageBox,
        ModalProgressCancelMessageBox,

        // Plugin Wrapper 
        PluginResponseMessage,
        PluginResponseError,
        PluginResponseWarning,
        PluginResponseWarningWithOption,
        PluginResponseOther
    }
}
