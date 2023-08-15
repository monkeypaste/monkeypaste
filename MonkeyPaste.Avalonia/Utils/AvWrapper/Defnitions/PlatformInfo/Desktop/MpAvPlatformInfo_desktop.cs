namespace MonkeyPaste.Avalonia {
    public class MpAvPlatformInfo_desktop : MpAvPlatformInfoBase {
        public override bool IsTouchInputEnabled =>
            false;// App.Args.Contains(App.MULTI_TOUCH_ARG);
    }
}
