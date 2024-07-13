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

        UpdateAvailable,
        NoUpdateAvailable,

        AppendModeChanged,
        TriggerEnabled,
        AlertAction,
        StartupComplete,
        FileIoWarning,

        // User Action (System Tray)
        UnloadPluginError,
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
        SingleInstanceWarning,

        // User Action (Modal) 

        ModalResetSharedValuePreset,
        ModalContentFormatDegradation,
        ModalYesNoCancelMessageBox,
        ModalYesNoMessageBox,
        ModalOkCancelMessageBox,
        ModalOkMessageBox,
        ModalTextBoxOkCancelMessageBox,
        ModalRememberableTextBoxOkCancelMessageBox,
        ModalProgressCancelMessageBox,
        ModalBusyMessageBox,
        ModalRestartIgnore,
        ModalRestartNowOrLater,
        ModalShutdownLater,
        ModalSettingsFrameMenu,

        ConfirmEndAppend,

        // Plugin Wrapper 
        PluginResponseMessage,
        PluginResponseError,
        PluginResponseWarning,
        PluginResponseWarningWithOption,
    }
}
