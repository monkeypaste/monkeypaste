namespace MonkeyPaste.Common {
    public enum MpThemeResourceKey {
        #region OPACITY

        GlobalBgOpacity_desktop,
        GlobalBgOpacity_browser,
        GlobalBgOpacity_mobile,
        GlobalBgOpacity,

        #endregion

        #region COLOR PALETTE
        PreThemeIdx,

        ThemeColor,

        ThemeBlackColor,
        ThemeWhiteColor,

        ThemeInteractiveColor,
        ThemeInteractiveColor_norand, // only used for tinting to force actual fg color
        ThemeInteractiveBgColor,

        ThemeNoAccentColor,
        ThemeAccent1Color,
        ThemeAccent1BgColor,
        ThemeAccent2Color,
        ThemeAccent2BgColor,
        ThemeAccent3Color,
        ThemeAccent3BgColor,
        ThemeAccent4Color,
        ThemeAccent4BgColor,
        ThemeAccent5Color,
        ThemeAccent5BgColor,

        ThemeCompliment1Color,
        ThemeCompliment1DarkColor,
        ThemeCompliment2Color,
        ThemeCompliment2DarkColor,
        ThemeCompliment3Color,
        ThemeCompliment3DarkColor,
        ThemeCompliment4Color,
        ThemeCompliment4DarkColor,
        ThemeCompliment5Color,
        ThemeCompliment5DarkColor,
        ThemeCompliment5LighterColor,

        ThemeGrayAccent1Color,
        ThemeGrayAccent2Color,
        ThemeGrayAccent3Color,

        PostThemeIdx,
        #endregion

        #region FONTS

        DefaultEditableFontFamily,
        DefaultReadOnlyFontFamily,
        ContentControlThemeFontFamily,

        #endregion

        #region LAYOUT

        DefaultGridSplitterFixedDimensionLength_desktop,
        DefaultGridSplitterFixedDimensionLength_browser,
        DefaultGridSplitterFixedDimensionLength_mobile,
        DefaultGridSplitterFixedDimensionLength
        #endregion
    }
}
