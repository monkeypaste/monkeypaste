namespace MonkeyPaste.Common.Avalonia {
    public static class MpAvCommonTools {
        public static MpAvICommonTools Services { get; private set; }
        public static void Init(MpAvICommonTools services) {
            Services = services;
        }
    }
}
