namespace MonkeyPaste.Common.Plugin {
    public enum MpPluginNotificationType {
        // NOTE these need to match the enum names in MpNotificationType (in core)
        PluginResponseError,
        PluginResponseWarning,
        PluginResponseWarningWithOption,
        PluginResponseOther
    }
}