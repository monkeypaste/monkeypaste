using MrMeeseeks.ResXToViewModelGenerator;

namespace MonkeyPaste.Avalonia {
    public static class R {
        public static CurrentUiStringsViewModel U { get; set; }
        public static CurrentEnumUiStringsViewModel E { get; set; }
        static R() {
            U = new CurrentUiStringsViewModel();
            E = new CurrentEnumUiStringsViewModel();
        }
    }
}
