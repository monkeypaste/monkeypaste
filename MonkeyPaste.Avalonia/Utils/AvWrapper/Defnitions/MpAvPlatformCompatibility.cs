namespace MonkeyPaste.Avalonia {
    public class MpAvPlatformCompatibility : MpIPlatformCompatibility {
        public bool UseCefNet => MpAvCefNetApplication.UseCefNet;
        public bool AllowTransparency { get; set; } = true;
        public bool ShowAnimations { get; set; } = true;
    }
}
