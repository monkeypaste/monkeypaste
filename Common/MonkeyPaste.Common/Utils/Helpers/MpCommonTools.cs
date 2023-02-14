namespace MonkeyPaste.Common {
    public static class MpCommonTools {
        public static MpICommonTools Services { get; private set; }
        public static void Init(MpICommonTools services) {
            Services = services;
        }
    }
}
