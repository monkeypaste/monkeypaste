namespace MonkeyPaste {
    public interface MpIPlatformCompatibility {
        bool UseCefNet { get; }
        bool AllowTransparency { get; }
        bool ShowAnimations { get; }
    }
}
