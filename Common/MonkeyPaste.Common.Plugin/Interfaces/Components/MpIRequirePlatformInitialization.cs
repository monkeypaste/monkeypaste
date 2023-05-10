namespace MonkeyPaste.Common.Plugin {
    public interface MpIRequirePlatformInitialization {
        MpPluginRequireInitializationResponseFormat Intialize(MpPluginRequireInitializationRequestFormat req);
    }
}
