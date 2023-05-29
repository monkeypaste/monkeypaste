using Avalonia.Controls;
using Avalonia.Media;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Avalonia;
using MonoMac.CoreImage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace MonkeyPaste.Avalonia {
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

        ThemeBlack,
        ThemeWhite,

        ThemeInteractiveColor,
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

        ThemeGrayAccent1,
        ThemeGrayAccent2,
        ThemeGrayAccent3,
        ThemeGrayAccent4,

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

    enum EditorCssPropType {
        noselectbgcolor = 0,
        subselecteditorbgcolor,
        editableeditorbgcolor,
        editableeditorbgcolor_opaque,
        defcontentfgcolor,
        selfgcolor,
        caretcolor,
        editortoolbarbgcolor,
        editortoolbarsepbgcolor,
        editortoolbarbuttoncolor,
        pastetemplatebgcolor,
        pastetoolbarbuttoncolor,
        edittemplatebgcolor,
    };

    public class MpAvThemeViewModel : MpViewModelBase {
        #region Private Variable
        private string[] _themePrefPropNames = new string[] {
                nameof(MpPrefViewModel.Instance.DefaultReadOnlyFontFamily),
                nameof(MpPrefViewModel.Instance.DefaultEditableFontFamily),
                nameof(MpPrefViewModel.Instance.DefaultFontSize),
                nameof(MpPrefViewModel.Instance.GlobalBgOpacity),
                nameof(MpPrefViewModel.Instance.ThemeColor),
                nameof(MpPrefViewModel.Instance.ThemeTypeName)
            };

        private string _curEditorPaletteBase64Str;

        #endregion

        #region Constants
        #endregion

        #region Statics
        private static MpAvThemeViewModel _instance;
        public static MpAvThemeViewModel Instance => _instance ?? (_instance = new MpAvThemeViewModel());

        public void Init() {
            // empty
        }
        #endregion

        #region Properties

        #region Appearance

        #region Fixed Resources

        public int ShakeDurMs =>
            500;

        #endregion

        public double GlobalBgOpacity {
            get => GetThemeValue<double>(MpThemeResourceKey.GlobalBgOpacity);
            set {
                if (GlobalBgOpacity != value) {
                    double clamped_value = Math.Max(0, Math.Min(value, 1.0d));
                    SetThemeValue(MpThemeResourceKey.GlobalBgOpacity, clamped_value);
                    OnPropertyChanged(nameof(GlobalBgOpacity));
                }
            }
        }

        public string ThemeColor {
            get => GetThemeValue<Color>(MpThemeResourceKey.ThemeColor).ToPortableColor().ToHex();
            set {
                if (ThemeColor != value) {
                    SetThemeValue(MpThemeResourceKey.ThemeColor, value.ToAvColor());
                    OnPropertyChanged(nameof(ThemeColor));
                }
            }
        }

        public double DefaultGridSplitterFixedDimensionLength {
            get => GetThemeValue<double>(MpThemeResourceKey.DefaultGridSplitterFixedDimensionLength);
            set {
                if (DefaultGridSplitterFixedDimensionLength != value) {
                    SetThemeValue(MpThemeResourceKey.DefaultGridSplitterFixedDimensionLength, value);
                    OnPropertyChanged(nameof(DefaultGridSplitterFixedDimensionLength));
                }
            }
        }

        public string DefaultReadOnlyFontFamily {
            get => GetThemeValue<string>(MpThemeResourceKey.DefaultReadOnlyFontFamily);
            set {
                if (DefaultReadOnlyFontFamily != value) {
                    SetThemeValue(MpThemeResourceKey.DefaultReadOnlyFontFamily, value);
                    SetThemeValue(MpThemeResourceKey.ContentControlThemeFontFamily, value);
                    OnPropertyChanged(nameof(DefaultReadOnlyFontFamily));
                }
            }
        }

        public string DefaultEditableFontFamily {
            get => GetThemeValue<string>(MpThemeResourceKey.DefaultEditableFontFamily);
            set {
                if (DefaultEditableFontFamily != value) {
                    SetThemeValue(MpThemeResourceKey.DefaultEditableFontFamily, value);
                    OnPropertyChanged(nameof(DefaultEditableFontFamily));
                }
            }
        }

        #endregion

        #region State

        public bool IsDesktop =>
            Mp.Services != null &&
            Mp.Services.PlatformInfo != null &&
            Mp.Services.PlatformInfo.IsDesktop;

        public bool IsMobile =>
            Mp.Services != null &&
            Mp.Services.PlatformInfo != null &&
            Mp.Services.PlatformInfo.IsMobile;

        public bool IsBrowser =>
            Mp.Services != null &&
            Mp.Services.PlatformInfo != null &&
            Mp.Services.PlatformInfo.IsBrowser;

        #endregion

        #endregion

        #region Constructors
        private MpAvThemeViewModel() {
            PropertyChanged += MpAvThemeViewModel_PropertyChanged;
#if DESKTOP
            GlobalBgOpacity = GetThemeValue<double>(MpThemeResourceKey.GlobalBgOpacity_desktop); ;
            DefaultGridSplitterFixedDimensionLength = GetThemeValue<double>(MpThemeResourceKey.DefaultGridSplitterFixedDimensionLength_desktop);
#elif BROWSER
            GlobalBgOpacity = GetThemeValue<double>(MpThemeResourceKey.GlobalBgOpacity_browser); ;
            DefaultGridSplitterFixedDimensionLength = GetThemeValue<double>(MpThemeResourceKey.DefaultGridSplitterFixedDimensionLength_browser);
#else
            GlobalBgOpacity = GetThemeValue<double>(MpThemeResourceKey.GlobalBgOpacity_mobile); ;
            DefaultGridSplitterFixedDimensionLength = GetThemeValue<double>(MpThemeResourceKey.DefaultGridSplitterFixedDimensionLength_mobile);
#endif
            SyncThemePrefs();
        }
        #endregion

        #region Public Methods
        public void SyncThemePrefs() {
            _themePrefPropNames.ForEach(x => SyncThemePref(x));

            CreatePalette();
        }

        public bool IsThemePref(string prefName) {
            return _themePrefPropNames.Contains(prefName);
        }

        public string GetEditorThemeLookup() {
            if (_curEditorPaletteBase64Str == null) {
                CreatePalette();
            }

            return _curEditorPaletteBase64Str;
        }

        #endregion

        #region Protected Methods
        #endregion

        #region Private Methods
        private void MpAvThemeViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            switch (e.PropertyName) {
                case nameof(HasModelChanged):

                    break;
            }
        }

        private void SyncThemePref(string prefName) {
            if (!this.HasProperty(prefName)) {
                return;
            }
            this.SetPropertyValue(prefName, MpPrefViewModel.Instance.GetPropertyValue(prefName));
        }

        private T GetThemeValue<T>(MpThemeResourceKey trk) {
            return (T)Mp.Services.PlatformResource.GetResource(trk.ToString());
        }
        private void SetThemeValue(MpThemeResourceKey trk, object value) {
            Mp.Services.PlatformResource.SetResource(trk.ToString(), value);

        }

        private void CreatePalette() {
            // test: #b511db

            // triadic            
            // h1_tr = (h + 120) % 360
            // h2_tr = (h + 240) % 360

            // tetradic
            // h1_te = (h + 90) % 360
            // h2_te = (h + 180) % 360
            // h3_te = (h + 270) % 360

            // 0: main bg color (theme color) 
            // 1: title color h1_tr (accent1bg)
            // 2: selected list h2_tr (accent5bg)
            // 3: list item hover h3_te (accent5)
            // 4: selected hover h2_te (V=100) (accent3)
            // 5: selected nohover h1_te (V=100) (accent2)
            // 6: hover h1_tr (V=100) (accent1)
            // 7: pin tray bg h1_te(S -60) (theme compliment1)
            // 8: pin tray shadow bg h1_te(S -60, V - 45) (theme compliment1 dark)

            // 9: light grays => h(S=5) (gray accent2)
            // 10: dim grays => h(S=5,V-30) (gray accent1)
            // 11: grays => h(S=10, V-10) (gray accent3)
            // 12: bright grays => h(S=5, V-10) (gray accent4)

            // 13: theme black h(S=5, V=15)
            // 14: theme white h(S=5, V=95)

            // 15: can resize h1_te (H-30) (accent4)
            // 16: is resize h1_te (H-30, S=50,V=100) (accent4 bg)

            // 17: param bg h(H-15) (comp2)
            // 18: sel/hover param bg h(H-15,V=100) (comp2bg)

            // 19: criteria row1 h(s=30,v=90) (comp3)
            // 20: criteria row2 h(s=5,v=90) (comp3bg)

            // 21: interactiveFg
            // 22: interactiveBg


            string hex = ThemeColor;
            if (MpPrefViewModel.Instance.ThemeType != MpThemeType.Default) {
                hex.ToPortableColor().ColorToHsl(out double th, out double ts, out double tl);
                if (MpPrefViewModel.Instance.ThemeType == MpThemeType.Dark) {
                    tl = Math.Max(5d / 100d, tl - (15d / 100d));
                } else {
                    tl = Math.Min(95d / 100d, tl + (15d / 100d));
                }
                hex = MpColorHelpers.ColorFromHsl(th, ts, tl).ToHex(true);
            }

            hex.ToPortableColor().ColorToHsv(out double h, out double s, out double v);

            // triadic    
            double h1_tr = (h + 120).Wrap(0, 360);
            double h2_tr = (h + 240).Wrap(0, 360);

            // tetradic
            double h1_te = (h + 90).Wrap(0, 360);
            double h2_te = (h + 180).Wrap(0, 360);
            double h3_te = (h + 270).Wrap(0, 360);


            // *HSV based*
            // 0
            List<string> palette = new List<string>() { hex };
            // 1
            palette.Add(MpColorHelpers.ColorFromHsv(h1_tr, s, v).ToHex(true));
            // 2
            palette.Add(MpColorHelpers.ColorFromHsv(h2_tr, s, v).ToHex(true));
            // 3
            palette.Add(MpColorHelpers.ColorFromHsv(h3_te, s, v).ToHex(true));
            // 4
            palette.Add(MpColorHelpers.ColorFromHsv(h2_te, s, 1d).ToHex(true));
            // 5
            palette.Add(MpColorHelpers.ColorFromHsv(h1_te, s, 1d).ToHex(true));
            // 6
            palette.Add(MpColorHelpers.ColorFromHsv(h1_tr, s, 1d).ToHex(true));
            // 7
            palette.Add(MpColorHelpers.ColorFromHsv(h1_te, Math.Max(0, s - (60d / 100)), v).ToHex(true));
            // 8
            palette.Add(MpColorHelpers.ColorFromHsv(h1_te, Math.Max(0, s - (60d / 100)), Math.Max(0, v - (45d / 100d))).ToHex(true));
            // 9
            palette.Add(MpColorHelpers.ColorFromHsv(h, 5d / 100, v).ToHex(true));
            // 10
            palette.Add(MpColorHelpers.ColorFromHsv(h, 5d / 100, Math.Max(0, v - (30d / 100d))).ToHex(true));
            // 11
            palette.Add(MpColorHelpers.ColorFromHsv(h, 10d / 100, Math.Max(0, v - (10d / 100d))).ToHex(true));
            // 12
            palette.Add(MpColorHelpers.ColorFromHsv(h, 5d / 100, Math.Max(0, v - (10d / 100d))).ToHex(true));
            // 13
            palette.Add(MpColorHelpers.ColorFromHsv(h, 5d / 100, 15d / 100d).ToHex(true));
            // 14
            palette.Add(MpColorHelpers.ColorFromHsv(h, 5d / 100, 95d / 100d).ToHex(true));
            // 15
            palette.Add(MpColorHelpers.ColorFromHsv((h1_te - 30d).Wrap(0, 360), s, v).ToHex(true));
            // 16
            palette.Add(MpColorHelpers.ColorFromHsv((h1_te - 30d).Wrap(0, 360), 50d / 100d, 1d).ToHex(true));
            // 17
            palette.Add(MpColorHelpers.ColorFromHsv((h - 15d).Wrap(0, 360), s, v).ToHex(true));
            // 18
            palette.Add(MpColorHelpers.ColorFromHsv((h - 15d).Wrap(0, 360), s, 1d).ToHex(true));
            // 19
            palette.Add(MpColorHelpers.ColorFromHsv(h, 30d / 100d, 90d / 100d).ToHex(true));
            // 20
            palette.Add(MpColorHelpers.ColorFromHsv(h, 5d / 100d, 90d / 100d).ToHex(true));

            if (MpPrefViewModel.Instance.ThemeType == MpThemeType.Dark) {
                // 21 (fg)
                palette.Add(palette[14]);
                // 22 (bg)
                palette.Add(palette[13]);
            } else {
                // 21 (fg)
                palette.Add(palette[13]);
                // 22 (bg)
                palette.Add(palette[14]);
            }

            ThemeColor = palette[0];
            var colors = palette.Select(x => x.ToAvColor()).ToArray();
            SetThemeValue(MpThemeResourceKey.ThemeAccent1BgColor, colors[1]);
            SetThemeValue(MpThemeResourceKey.ThemeAccent5BgColor, colors[2]);
            SetThemeValue(MpThemeResourceKey.ThemeAccent5Color, colors[3]);
            SetThemeValue(MpThemeResourceKey.ThemeAccent3Color, colors[4]);
            SetThemeValue(MpThemeResourceKey.ThemeAccent2Color, colors[5]);
            SetThemeValue(MpThemeResourceKey.ThemeAccent1Color, colors[6]);
            SetThemeValue(MpThemeResourceKey.ThemeCompliment1Color, colors[7]);
            SetThemeValue(MpThemeResourceKey.ThemeCompliment1DarkColor, colors[8]);

            SetThemeValue(MpThemeResourceKey.ThemeGrayAccent1, colors[9]);
            SetThemeValue(MpThemeResourceKey.ThemeGrayAccent2, colors[10]);
            SetThemeValue(MpThemeResourceKey.ThemeGrayAccent3, colors[11]);
            SetThemeValue(MpThemeResourceKey.ThemeGrayAccent4, colors[12]);

            SetThemeValue(MpThemeResourceKey.ThemeBlack, colors[13]);
            SetThemeValue(MpThemeResourceKey.ThemeWhite, colors[14]);

            SetThemeValue(MpThemeResourceKey.ThemeAccent4Color, colors[15]);
            SetThemeValue(MpThemeResourceKey.ThemeAccent4BgColor, colors[16]);

            SetThemeValue(MpThemeResourceKey.ThemeCompliment2Color, colors[17]);
            SetThemeValue(MpThemeResourceKey.ThemeCompliment2DarkColor, colors[18]);

            SetThemeValue(MpThemeResourceKey.ThemeCompliment3Color, colors[19]);
            SetThemeValue(MpThemeResourceKey.ThemeCompliment3DarkColor, colors[20]);

            SetThemeValue(MpThemeResourceKey.ThemeInteractiveColor, colors[21]);
            SetThemeValue(MpThemeResourceKey.ThemeInteractiveBgColor, colors[22]);
        }
        #endregion

        #region Commands
        #endregion
    }
}
