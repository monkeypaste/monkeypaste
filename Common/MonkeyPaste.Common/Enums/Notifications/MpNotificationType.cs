namespace MonkeyPaste.Common {
    public enum MpNotificationType {
        None = 0,
        // Loader
        Loader,

        SubscriptionExpired,
        AccountLoginFailed,

        // Message
        DbError,
        Help,
        PluginUpdated,
        Message,
        AppModeChange,
        Debug,

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
        RateApp,

        // User Action (Modal) 

        ModalContentFormatDegradation,
        ModalYesNoCancelMessageBox,
        ModalYesNoMessageBox,
        ModalOkCancelMessageBox,
        ModalOkMessageBox,
        ModalTextBoxOkCancelMessageBox,
        ModalRememberableTextBoxOkCancelMessageBox,
        ModalProgressCancelMessageBox,
        ModalBusyMessageBox,

        ConfirmEndAppend,

        // Plugin Wrapper 
        PluginResponseMessage,
        PluginResponseError,
        PluginResponseWarning,
        PluginResponseWarningWithOption,
        PluginResponseOther
    }
}
