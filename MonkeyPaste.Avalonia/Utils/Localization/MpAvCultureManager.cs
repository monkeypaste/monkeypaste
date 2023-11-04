using System.Globalization;

namespace MonkeyPaste.Avalonia {
    public static class MpAvCultureManager {
        public const string DEFAULT_CULTURE_NAME = "en-US";
        public static void SetCulture(string culture) {
            UiStrings.Culture = new CultureInfo(culture);
            EnumUiStrings.Culture = new CultureInfo(culture);
        }
        public static bool IsDefaultCulture(CultureInfo c) {
            return c.Name == DEFAULT_CULTURE_NAME;
        }

    }
}
