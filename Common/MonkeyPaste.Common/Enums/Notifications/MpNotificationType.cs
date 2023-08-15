namespace MonkeyPaste.Common {
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
        AccountChanged,

        Welcome,

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
        DbPasswordInput,

        // User Action (Modal) 

        ModalContentFormatDegradation,
        ModalYesNoCancelMessageBox,
        ModalYesNoMessageBox,
        ModalOkCancelMessageBox,
        ModalOkMessageBox,
        ModalTextBoxOkCancelMessageBox,
        ModalProgressCancelMessageBox,

        ConfirmEndAppend,

        // Plugin Wrapper 
        PluginResponseMessage,
        PluginResponseError,
        PluginResponseWarning,
        PluginResponseWarningWithOption,
        PluginResponseOther
    }
}
