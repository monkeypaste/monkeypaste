namespace MonkeyPaste.Avalonia {
    public static class R {
        //public static CurrentUiStringsViewModel U { get; set; }
        //public static CurrentEnumUiStringsViewModel E { get; set; }
        //static R() {
        //    U = new CurrentUiStringsViewModel();
        //    E = new CurrentEnumUiStringsViewModel();
        //}

        public static TempResxWrapper U { get; set; }
        static R() {
            U = new TempResxWrapper();
        }
    }
    public class TempResxWrapper : MpAvViewModelBase {
        public UiStrings CurrentUiStrings { get; set; }
        public TempResxWrapper() {
            CurrentUiStrings = new UiStrings();
        }
    }

}
