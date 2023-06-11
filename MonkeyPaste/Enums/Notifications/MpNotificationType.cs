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
        UserTriggerEnabled,
        UserTriggerDisabled,
        AppModeChange,
        TrialExpired,
        ContentCapReached,
        TrashCapReached,

        // User Action (System Tray)
        InvalidPlugin,
        InvalidClipboardFormatHandler,
        InvalidAction,
        BadHttpRequest,
        AnalyzerTimeout,
        InvalidRequest,
        InvalidResponse,
        FileIoError,
        ExecuteParametersRequest,

        // User Action (Modal) 

        ModalContentFormatDegradation,
        ModalYesNoCancelMessageBox,
        ModalYesNoMessageBox,
        ModalOkCancelMessageBox,
        ModalOkMessageBox,
        ModalTextBoxOkCancelMessageBox,

        // Plugin Wrapper 
        PluginResponseMessage,
        PluginResponseError,
        PluginResponseWarning,
        PluginResponseWarningWithOption,
        PluginResponseOther
    }
}
