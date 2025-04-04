﻿namespace MonkeyPaste.Common {
    public enum MpThemeResourceKey {
        #region OPACITY

        GlobalBgOpacity_desktop,
        GlobalBgOpacity_browser,
        GlobalBgOpacity_mobile,
        GlobalBgOpacity,
        GlobalInteractiveBgOpacity,
        GlobalDisabledOverlayOpacity,
        GlobalInteractiveOpacity,

        #endregion

        #region COLOR PALETTE
        PreThemeIdx,

        ThemeColor,
        ThemeLightColor,
        ThemeDarkColor,

        ThemeLightBgColor,
        ThemeDarkBgColor,

        ThemeHiColor,
        ThemeLoColor,
        ThemeHi1Color,
        ThemeLo1Color,
        ThemeHi2Color,
        ThemeLo2Color,
        ThemeHi3Color,
        ThemeLo3Color,
        ThemeHi4Color,
        ThemeLo4Color,
        ThemeHi5Color,
        ThemeLo5Color,

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

        ThemePasteToolbarBgColor,
        ThemePasteButtonDefaultBgColor,
        ThemePasteButtonCustomBgColor,

        ThemeContentLinkColor,
        ThemeContentLinkHoverColor,

        ThemeHighlightInactiveColor,
        ThemeHighlightActiveColor,

        PostThemeIdx,
        #endregion

        #region FONTS

        DefaultEditableFontFamily,
        DefaultEditableFontFamilyFont,
        DefaultReadOnlyFontFamily,
        DefaultReadOnlyFontFamilyFont,
        ContentControlThemeFontFamily,
        IsRtl,

        #endregion

        #region LAYOUT

        DefaultGridSplitterFixedDimensionLength_desktop,
        DefaultGridSplitterFixedDimensionLength_browser,
        DefaultGridSplitterFixedDimensionLength_mobile,
        DefaultGridSplitterFixedDimensionLength,
        DefaultNotificationWidth,
        DefaultNotificationHeight,
        #endregion

        #region Effects

        ThemeBlackDropShadow,
        ThemeWhiteDropShadow
        #endregion
    }
}
