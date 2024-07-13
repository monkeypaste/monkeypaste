namespace MonkeyPaste.Avalonia {
    public class MpAvPlatformInfo_desktop : MpAvPlatformInfoBase {
        public override bool IsTouchInputEnabled =>
#if DEBUG
            true;// App.Args.Contains(App.MULTI_TOUCH_ARG);
#else
            false;
#endif
    }
}
