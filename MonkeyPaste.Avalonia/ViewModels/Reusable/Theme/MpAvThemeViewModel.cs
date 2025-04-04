﻿using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Layout;
using Avalonia.Markup.Xaml.MarkupExtensions;
using Avalonia.Media;
using Avalonia.Threading;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Avalonia;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MonkeyPaste.Avalonia {

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

    public class MpAvThemeViewModel :
        MpAvViewModelBase {

        #region Private Variable

        private string[] _themePrefPropNames = new string[] {
                nameof(MpAvPrefViewModel.Instance.DefaultReadOnlyFontFamily),
                nameof(MpAvPrefViewModel.Instance.DefaultEditableFontFamily),
                nameof(MpAvPrefViewModel.Instance.DefaultFontSize),
                nameof(MpAvPrefViewModel.Instance.GlobalBgOpacity),
                nameof(MpAvPrefViewModel.Instance.ThemeColor),
                nameof(MpAvPrefViewModel.Instance.ThemeTypeName)
            };

        private string[] _colorImageFileNames = new string[] {
            "add_green.png",
            "appstore.png",
            "bell.png",
            "banana.png",
            "butterfly.png",
            "clipboard.png",
            "close.png",
            "cogcolor.png",
            "colors.png",
            "delete.png",
            "delete2.png",
            "device.png",
            "egg.png",
            "error.png",
            "folder.png",
            "gavel.png",
            "ghost.png",
            "global.png",
            "graph.png",
            "heart.png",
            "html.png",
            "info.png",
            "joystickative.png",
            "joystickative2.png",
            "keyboardcolor.png",
            "ladybug.png",
            "letter.png",
            "lifepreserver.png",
            "log.png",
            "megaphone.png",
            "monkey.png",
            "monkeywink.png",
            "monkeyupdate.png",
            "noentry.png",
            "private.png",
            "pindown.png",
            "pindownover.png",
            "pinover.png",
            "questionmark.png",
            "radar.png",
            "read.png",
            "recyclebin.png",
            "remove.png",
            "robotarmcolor.png",
            "select.png",
            "sheep.png",
            "sliderscolor.png",
            "spellcheck.png",
            "stargold.png",
            "staryellow.png",
            "staryellow2.png",
            "tagcolor.png",
            "text.png",
            "trophy.png",
            "usercolor.png",
            "warning.png",
            "warningtime.png",
        };


        #endregion

        #region Constants
        public const double PHI = 1.618033988749894d;
        #endregion

        #region Statics
        public static bool IS_WINDOW_FADE_ENABLED = true;

        private static MpAvThemeViewModel _instance;
        public static MpAvThemeViewModel Instance => _instance ??= new MpAvThemeViewModel();

        public void Init() {
            UpdateThemeResources();
        }
        #endregion

        #region Interfaces
        #endregion

        #region Properties

        #region Layout

        public Orientation Orientation =>
            //Mp.Services == null ||
            //Mp.Services.StartupState == null ||
           // !Mp.Services.StartupState.IsReady ||
            MpAvMainWindowViewModel.Instance.IsVerticalOrientation ?
                Orientation.Vertical : Orientation.Horizontal; 

        #endregion

        #region Appearance



        public bool IsRtl {
            get =>
                GetThemeValue<bool>(MpThemeResourceKey.IsRtl);
            set {
                if (IsRtl != value) {
                    SetThemeValue(MpThemeResourceKey.IsRtl, value);
                    OnPropertyChanged(nameof(IsRtl));
                }
            }
        }
        public double GlobalBgOpacity {
            get =>
                GetThemeValue<double>(MpThemeResourceKey.GlobalBgOpacity);
            set {
                if (GlobalBgOpacity != value) {
                    double clamped_value = Math.Max(0, Math.Min(value, 1.0d));
                    SetThemeValue(MpThemeResourceKey.GlobalBgOpacity, clamped_value);
                    OnPropertyChanged(nameof(GlobalBgOpacity));
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
                    FontFamily ff = MpAvStringToFontFamilyConverter.Instance.Convert(value,
                        typeof(FontFamily), null, CultureInfo.CurrentCulture) as FontFamily;
                    SetThemeValue(MpThemeResourceKey.DefaultReadOnlyFontFamilyFont, ff);
                    OnPropertyChanged(nameof(DefaultReadOnlyFontFamily));
                }
            }
        }

        public string DefaultEditableFontFamily {
            get => GetThemeValue<string>(MpThemeResourceKey.DefaultEditableFontFamily);
            set {
                if (DefaultEditableFontFamily != value) {
                    SetThemeValue(MpThemeResourceKey.DefaultEditableFontFamily, value);
                    FontFamily ff = MpAvStringToFontFamilyConverter.Instance.Convert(value,
                        typeof(FontFamily), null, CultureInfo.CurrentCulture) as FontFamily;
                    SetThemeValue(MpThemeResourceKey.DefaultEditableFontFamilyFont, ff);
                    OnPropertyChanged(nameof(DefaultEditableFontFamily));
                }
            }
        }

        #endregion

        #region State

        public string PlatformShortName =>
#if WINDOWS
            "windows";
#elif MAC
            "mac";
#elif LINUX
            "linux";
#elif ANDROID
            "android";
#elif IOS
            "ios";
#else
                    "";
#endif

        public bool IsDesktop =>
#if DESKTOP
            true;
#else
            false;
#endif
        public bool IsMobile =>
#if MOBILE
            true;
#else
            false;
#endif
        public bool IsWindowed =>
#if WINDOWED
            true;
#else
            false;// MpAvPrefViewModel.Instance == null ? false : MpAvPrefViewModel.Instance.IsWindowed;
#endif

        public bool IsMobileOrWindowed =>
            IsMobile || IsWindowed;

        public bool IsMultiWindow =>
            !IsMobileOrWindowed;

        public bool IsBrowser =>
#if BROWSER
            true;
#else
            false;
#endif

        public bool IsMac =>
#if MAC
            true;
#else
            false;
#endif

        public int ShakeDurMs =>
            500;

        public int FocusPulseDurMs =>
            3_000;
        public string[] CustomFontFamilyNames => new string[] {
                "Habanero",
                "Nunito"
            };

        public bool IsThemeDark =>
            MpAvPrefViewModel.Instance.IsThemeDark;

        public bool IsThemeLight =>
            !MpAvPrefViewModel.Instance.IsThemeDark;
        #endregion

        #endregion

        #region Constructors
        private MpAvThemeViewModel() {
            InitDefaults();
            UpdateThemeResources();
        }
        #endregion

        #region Public Methods
        public void UpdateThemeResources(bool showWait = false) {
            _themePrefPropNames.ForEach(x => SyncThemePref(x));

            if (showWait) {
                // TODO create cancellationToken and show busy mtf here
            }

            CreatePalette();
            if (showWait) {
                // TODO cancel token here...
            }
        }

        public bool IsThemePref(string prefName) {
            return _themePrefPropNames.Contains(prefName);
        }

        public bool IsColoredImageResource(object resource_key_or_uri) {
            if (resource_key_or_uri is not string res_str ||
                res_str.IsStringBase64()) {
                return true;
            }

            if (string.IsNullOrEmpty(res_str)) {
                return false;
            }
            if (res_str.EndsWith("Image")) {
                if(Mp.Services == null ||
                    Mp.Services.PlatformResource == null) {
                    return false;
                }
                res_str = Mp.Services.PlatformResource.GetResource<string>(res_str);
            }
            return _colorImageFileNames.Any(x => res_str.ToLowerInvariant().EndsWith(x));
        }

        public void HandlePulse(MpAvIPulseViewModel pvm) {
            if (!pvm.DoFocusPulse) {
                return;
            }
            Dispatcher.UIThread.Post(async () => {
                var sw = Stopwatch.StartNew();
                while (sw.ElapsedMilliseconds < FocusPulseDurMs) {
                    await Task.Delay(100);
                }
                pvm.DoFocusPulse = false;
            });
        }
        #endregion

        #region Protected Methods
        #endregion

        #region Private Methods

        private void InitDefaults() {

#if MULTI_WINDOW
            GlobalBgOpacity = GetThemeValue<double>(MpThemeResourceKey.GlobalBgOpacity_desktop); ;
            DefaultGridSplitterFixedDimensionLength = GetThemeValue<double>(MpThemeResourceKey.DefaultGridSplitterFixedDimensionLength_desktop);
#elif BROWSER
            GlobalBgOpacity = GetThemeValue<double>(MpThemeResourceKey.GlobalBgOpacity_browser); ;
            DefaultGridSplitterFixedDimensionLength = GetThemeValue<double>(MpThemeResourceKey.DefaultGridSplitterFixedDimensionLength_browser);
#else
            GlobalBgOpacity = GetThemeValue<double>(MpThemeResourceKey.GlobalBgOpacity_mobile, 1);
            DefaultGridSplitterFixedDimensionLength = GetThemeValue<double>(MpThemeResourceKey.DefaultGridSplitterFixedDimensionLength_mobile, 0);
#endif
        }

        private void SyncThemePref(string prefName) {
            if (!this.HasProperty(prefName)) {
                return;
            }
            this.SetPropertyValue(prefName, MpAvPrefViewModel.Instance.GetPropertyValue(prefName));
        }

        private T GetThemeValue<T>(MpThemeResourceKey trk, T fallback = default) {
            if (Mp.Services == null ||
                Mp.Services.PlatformResource == null) {
                return fallback;
            }
            return (T)Mp.Services?.PlatformResource?.GetResource(trk.ToString());
        }

        private void SetThemeValue(MpThemeResourceKey trk, object value) {            
            Mp.Services?.PlatformResource?.SetResource(trk.ToString(), value, false);
        }
        
        private void SetThemeColor(MpThemeResourceKey trk, Color color, MpThemeResourceKey opacityKey = MpThemeResourceKey.GlobalBgOpacity) {
            // set theme color resource
            string color_key = trk.ToString();
            Mp.Services?.PlatformResource?.SetResource(color_key, color, true);

            // create brush using color and opacity bindings
            SolidColorBrush scb = new SolidColorBrush() {
                Color = color,
                Opacity = GetThemeValue<double>(opacityKey,1)
            };
            string brush_key = color_key.Replace("Color", "Brush");
            Mp.Services?.PlatformResource?.SetResource(brush_key, scb, true);
        }

        private string GetThemeCss() {
            var sb = new StringBuilder();
            var props = new Dictionary<string, string>();
            foreach(MpThemeResourceKey key in Enum.GetValues(typeof(MpThemeResourceKey))) {
                if(!key.ToString().EndsWith("Color")) {
                    continue;
                }
                var valObj = Mp.Services?.PlatformResource?.GetResource(key.ToString());
                string hex = null;
                if (valObj is IBrush b) {
                    hex = b.ToHex();
                } else if (valObj is Color c) {
                    hex = c.ToHex();
                }
                if(string.IsNullOrWhiteSpace(hex)) {
                    continue;
                }
                props.AddOrReplace(key.ToString(), hex);

            }
            //foreach(var kvp in Application.Current.Resources) {
            //    string hex = null;
            //    var valObj = kvp.Value;
            //    if (valObj is SolidColorBrush scb) {
            //        hex = scb.ToHex();
            //    } else if (valObj is Color c) {
            //        hex = c.ToHex();
            //    } else {
            //        continue;
            //    }
            //    if (string.IsNullOrWhiteSpace(hex)) {
            //        continue;
            //    }
            //    props.AddOrReplace(kvp.Key.ToString(), hex);
            //}
            props.OrderBy(x => x.Key).ForEach(x => sb.AppendLine($".{x.Key} {{ background-color: {MpColorHelpers.HexToWebHex(x.Value)}}}"));
            return sb.ToString();
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

            // hexadic
            // h1_hx = (h + 60) % 360
            // h5_hx = (h + 300) % 360

            // octadic
            // h1_oc = (h + 45) % 360
            // h3_oc = (h + 135) % 360
            // h4_oc = (h + 180) % 360
            // h5_oc = (h + 225) % 360
            // h7_oc = (h + 315) % 360

            // 0: main bg color (theme color) 
            // 1: title color h1_tr (accent1bg)
            // 2: selected list h2_tr (S (accent5bg)
            // 3: list item hover h3_te (accent5)
            // 4: selected hover h2_te (V=100) (accent3)
            // 5: selected nohover h1_te (V=100) (accent2)
            // 6: hover h3_oc (V=100) (accent1)
            // 7: pin tray bg h1_te(S -60) (theme compliment1)
            // 8: pin tray shadow bg h1_te(S -60, V - 45) (theme compliment1 dark)

            // 9: dim grays => h(S=5,V-30) (gray accent1)
            // 10: grays => h(S=5, V-10) (gray accent2)
            // 11: light grays => h(S=10, V-10) (gray accent3)

            // 12: theme black h(S=5, V=15)
            // 13: theme white h(S=5, V=95)

            // 14: can resize h1_te (H-30) (accent4)
            // 15: is resize h1_te (H-30, S=50,V=100) (accent4 bg)

            // 16: param bg h(H-15) (comp2)
            // 17: sel/hover param bg h(H-15,V=100) (comp2bg)

            // 18: criteria row1 h(s=30,v=90) (comp3)
            // 19: criteria row2 h(s=5,v=90) (comp3bg)

            // 20: interactiveFg
            // 21: interactiveBg

            // 22: tooltip fg h1_tr (comp4)
            // 23: tooltip bg h1_tr(S=50,V=100) (comp4bg)

            // 24: default button bg h(h-240, S=15, V=95) (comp5)
            // 25: default button bg h(h-240, S=15, V=65) (comp5bg)

            if (MpAvPrefViewModel.Instance == null) {
                // null during global style init
                return;
            }

            MpThemeType tt = MpAvPrefViewModel.Instance.ThemeType;
            bool is_dark = tt == MpThemeType.Dark;
            string hex = MpAvPrefViewModel.Instance.ThemeColor;
            // prepass selected color to get decent chroma
            // V >= 50, S >= 50
            hex.ToPortableColor().ColorToHsv(out double preh, out double pres, out double prev);
            //pres = Math.Min(0.9d, Math.Max(0.5d, pres));
            prev = Math.Max(0.5d, prev);
            hex = MpColorHelpers.ColorFromHsv(preh, pres, prev).ToHex(true);
            hex.ToPortableColor().ColorToHsl(out double th, out double ts, out double tl);
            if (is_dark) {
                tl = Math.Max(25d / 100d, tl - (15d / 100d));
            } else {

                //    tl = Math.Min(75d / 100d, tl + (10d / 100d));
            }
            hex = MpColorHelpers.ColorFromHsl(th, ts, tl).ToHex(true);
            hex.ToPortableColor().ColorToHsv(out double h, out double s, out double v);

            // triadic    
            double h1_tr = (h + 120).Wrap(0, 360);
            double h2_tr = (h + 240).Wrap(0, 360);

            // tetradic
            double h1_te = (h + 90).Wrap(0, 360);
            double h2_te = (h + 180).Wrap(0, 360);
            double h3_te = (h + 270).Wrap(0, 360);

            // oxadic
            double h3_oc = (h + 135).Wrap(0, 360);


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
            palette.Add(MpColorHelpers.ColorFromHsv(h3_oc, s, 1d).ToHex(true));
            // 7
            palette.Add(MpColorHelpers.ColorFromHsv(h1_te, Math.Max(0, s - 0.6d), v).ToHex(true));
            // 8
            palette.Add(MpColorHelpers.ColorFromHsv(h1_te, Math.Max(0, s - 0.6d), Math.Max(0, v - 0.45d)).ToHex(true));

            // 9, 10, 11

            // when theme CHANGED from default/light to dark or vice versa swap most/least darkest gray references
            //string dark_gray = MpColorHelpers.ColorFromHsv(h, 0.05d, Math.Max(0, v - 0.3d)).ToHex(true); 
            //string med_gray = MpColorHelpers.ColorFromHsv(h, 0.05d, Math.Max(0, v - 0.1d)).ToHex(true); 
            //string light_gray = MpColorHelpers.ColorFromHsv(h, 0.05d, 0.9d).ToHex(true); 
            double gray_s = 0.03d;
            string dark_gray = MpColorHelpers.ColorFromHsv(h, gray_s, 0.32).ToHex(true);
            string med_gray = MpColorHelpers.ColorFromHsv(h, gray_s, 0.71).ToHex(true);
            string light_gray = MpColorHelpers.ColorFromHsv(h, gray_s, 0.86d).ToHex(true);
            if (tt == MpThemeType.Light) {
                palette.AddRange(new[] { dark_gray, med_gray, light_gray });
            } else {
                palette.AddRange(new[] { light_gray, med_gray, dark_gray });
            }

            // 12
            palette.Add(MpColorHelpers.ColorFromHsv(h, 0.1d, 0.15d).ToHex(true));
            // 13
            palette.Add(MpColorHelpers.ColorFromHsv(h, 0.01d, 0.999d).ToHex(true));
            // 14
            palette.Add(MpColorHelpers.ColorFromHsv((h1_te - 30d).Wrap(0, 360), s, v).ToHex(true));
            // 15
            palette.Add(MpColorHelpers.ColorFromHsv((h1_te - 30d).Wrap(0, 360), 0.5d, 1d).ToHex(true));
            // 16
            palette.Add(MpColorHelpers.ColorFromHsv((h - 15d).Wrap(0, 360), s, v).ToHex(true));
            // 17
            palette.Add(MpColorHelpers.ColorFromHsv((h - 15d).Wrap(0, 360), s, 1d).ToHex(true));
            // 18
            palette.Add(MpColorHelpers.ColorFromHsv(h, 0.3d, 0.9d).ToHex(true));
            // 19
            palette.Add(MpColorHelpers.GetDarkerHexColor(MpColorHelpers.ColorFromHsv(h, 0.3d, 0.9d).ToHex(true)));

            if (is_dark) {
                // 20 (fg)
                palette.Add(palette[13]);
                // 21 (bg)
                palette.Add(palette[12]);
            } else {
                // 20 (fg)
                palette.Add(palette[12]);
                // 21 (bg)
                palette.Add(palette[13]);
            }

            // 22
            palette.Add(MpColorHelpers.ColorFromHsv(h1_tr, s, v).ToHex(true));
            // 23
            palette.Add(MpColorHelpers.ColorFromHsv(h1_tr, 0.5d, 1.0d).ToHex(true));
            // 24
            palette.Add(MpColorHelpers.ColorFromHsv((h - 240.0d).Wrap(0, 360), 0.15d, 0.95d).ToHex(true));
            // 25
            palette.Add(MpColorHelpers.ColorFromHsv((h - 240.0d).Wrap(0, 360), 0.15d, 0.65d).ToHex(true));
            // 26
            palette.Add(MpColorHelpers.ColorFromHsv((h - 240.0d).Wrap(0, 360), 0.05d, 0.95d).ToHex(true));
            // 27
            palette.Add(MpColorHelpers.ColorFromHsl(th, ts, 0.8d).ToHex(true));
            // 28
            palette.Add(MpColorHelpers.ColorFromHsl(th, ts, 0.2d).ToHex(true));

            double mute_s = MpAvBrushToMutedBrushConverter.DEF_MUTE_S;
            double mute_l_hi = MpAvBrushToMutedBrushConverter.DEF_MUTE_L_HI;
            double mute_l_lo = MpAvBrushToMutedBrushConverter.DEF_MUTE_L_LO;
            // 29
            palette.Add(MpColorHelpers.ColorFromHsl(th, mute_s, mute_l_hi).ToHex(true));
            // 30
            palette.Add(MpColorHelpers.ColorFromHsl(th, mute_s, mute_l_lo).ToHex(true));

            // 31
            palette.Add(MpColorHelpers.ColorFromHsl(h3_oc, mute_s, mute_l_hi).ToHex(true));
            // 32
            palette.Add(MpColorHelpers.ColorFromHsl(h3_oc, mute_s, mute_l_lo).ToHex(true));
            
            // 33
            palette.Add(MpColorHelpers.ColorFromHsl(h1_te, mute_s, mute_l_hi).ToHex(true));
            // 34
            palette.Add(MpColorHelpers.ColorFromHsl(h1_te, mute_s, mute_l_lo).ToHex(true));
            
            // 35
            palette.Add(MpColorHelpers.ColorFromHsl(h2_te, mute_s, mute_l_hi).ToHex(true));
            // 36
            palette.Add(MpColorHelpers.ColorFromHsl(h2_te, mute_s, mute_l_lo).ToHex(true));

            // 37
            palette.Add(MpColorHelpers.ColorFromHsl((h1_te - 30d).Wrap(0, 360), mute_s, mute_l_hi).ToHex(true));
            // 38
            palette.Add(MpColorHelpers.ColorFromHsl((h1_te - 30d).Wrap(0, 360), mute_s, mute_l_lo).ToHex(true));
            
            // 39
            palette.Add(MpColorHelpers.ColorFromHsl(h3_te, mute_s, mute_l_hi).ToHex(true));
            // 40
            palette.Add(MpColorHelpers.ColorFromHsl(h3_te, mute_s, mute_l_lo).ToHex(true));

            var colors = palette.Select(x => x.ToAvColor()).ToArray();
            SetThemeColor(MpThemeResourceKey.ThemeColor, colors[0]);
            SetThemeColor(MpThemeResourceKey.ThemeAccent1BgColor, colors[1]);
            SetThemeColor(MpThemeResourceKey.ThemeAccent5BgColor, colors[2]);
            SetThemeColor(MpThemeResourceKey.ThemeAccent5Color, colors[3]);
            SetThemeColor(MpThemeResourceKey.ThemeAccent3Color, colors[4]);
            SetThemeColor(MpThemeResourceKey.ThemeAccent2Color, colors[5]);
            SetThemeColor(MpThemeResourceKey.ThemeAccent1Color, colors[6]);
            SetThemeColor(MpThemeResourceKey.ThemeCompliment1Color, colors[7]);
            SetThemeColor(MpThemeResourceKey.ThemeCompliment1DarkColor, colors[8]);

            SetThemeColor(MpThemeResourceKey.ThemeGrayAccent1Color, colors[9]);
            SetThemeColor(MpThemeResourceKey.ThemeGrayAccent2Color, colors[10]);
            SetThemeColor(MpThemeResourceKey.ThemeGrayAccent3Color, colors[11]);

            SetThemeColor(MpThemeResourceKey.ThemeBlackColor, colors[12]);
            SetThemeColor(MpThemeResourceKey.ThemeWhiteColor, colors[13]);

            SetThemeColor(MpThemeResourceKey.ThemeAccent4Color, colors[14]);
            SetThemeColor(MpThemeResourceKey.ThemeAccent4BgColor, colors[15]);

            SetThemeColor(MpThemeResourceKey.ThemeCompliment2Color, colors[16]);
            SetThemeColor(MpThemeResourceKey.ThemeCompliment2DarkColor, colors[17]);

            SetThemeColor(MpThemeResourceKey.ThemeCompliment3Color, colors[18]);
            SetThemeColor(MpThemeResourceKey.ThemeCompliment3DarkColor, colors[19]);

            SetThemeColor(MpThemeResourceKey.ThemeInteractiveColor, colors[20], MpThemeResourceKey.GlobalInteractiveOpacity);
            SetThemeColor(MpThemeResourceKey.ThemeInteractiveBgColor, colors[21], MpThemeResourceKey.GlobalInteractiveBgOpacity);

            SetThemeColor(MpThemeResourceKey.ThemeCompliment4Color, colors[22]);
            SetThemeColor(MpThemeResourceKey.ThemeCompliment4DarkColor, colors[23]);

            SetThemeColor(MpThemeResourceKey.ThemeCompliment5Color, colors[24]);
            SetThemeColor(MpThemeResourceKey.ThemeCompliment5DarkColor, colors[25]);
            SetThemeColor(MpThemeResourceKey.ThemeCompliment5LighterColor, colors[26]);

            SetThemeColor(MpThemeResourceKey.ThemeLightColor, colors[27]);
            SetThemeColor(MpThemeResourceKey.ThemeDarkColor, colors[28]);
            
            SetThemeColor(MpThemeResourceKey.ThemeLightBgColor, colors[29]);
            SetThemeColor(MpThemeResourceKey.ThemeDarkBgColor, colors[30]);

            SetThemeColor(MpThemeResourceKey.ThemeHiColor, is_dark ? colors[30] : colors[29]);
            SetThemeColor(MpThemeResourceKey.ThemeLoColor, is_dark ? colors[29] : colors[30]);

            SetThemeColor(MpThemeResourceKey.ThemeHi1Color, is_dark ? colors[32] : colors[31]);
            SetThemeColor(MpThemeResourceKey.ThemeLo1Color, is_dark ? colors[31] : colors[32]);

            SetThemeColor(MpThemeResourceKey.ThemeHi2Color, is_dark ? colors[34] : colors[33]);
            SetThemeColor(MpThemeResourceKey.ThemeLo2Color, is_dark ? colors[33] : colors[34]);

            SetThemeColor(MpThemeResourceKey.ThemeHi3Color, is_dark ? colors[36] : colors[35]);
            SetThemeColor(MpThemeResourceKey.ThemeLo3Color, is_dark ? colors[35] : colors[36]);

            SetThemeColor(MpThemeResourceKey.ThemeHi4Color, is_dark ? colors[38] : colors[37]);
            SetThemeColor(MpThemeResourceKey.ThemeLo4Color, is_dark ? colors[37] : colors[38]);

            SetThemeColor(MpThemeResourceKey.ThemeHi5Color, is_dark ? colors[40] : colors[39]);
            SetThemeColor(MpThemeResourceKey.ThemeLo5Color, is_dark ? colors[39] : colors[40]);

            // NON-DYNAMIC COLORS
            SetThemeValue(
                MpThemeResourceKey.ThemeContentLinkColor,
                is_dark ?
                    Mp.Services.PlatformResource.GetResource<IBrush>("ContentLinkColor_dark") :
                    Mp.Services.PlatformResource.GetResource<IBrush>("ContentLinkColor_light"));
            
            SetThemeValue(
                MpThemeResourceKey.ThemeContentLinkHoverColor,
                is_dark ?
                    Mp.Services.PlatformResource.GetResource<IBrush>("ContentLinkHoverColor_dark") :
                    Mp.Services.PlatformResource.GetResource<IBrush>("ContentLinkHoverColor_light"));

            SetThemeValue(
                MpThemeResourceKey.ThemePasteToolbarBgColor,
                is_dark ?
                    Mp.Services.PlatformResource.GetResource<Color>("PasteToolbarBgColor_dark") :
                    Mp.Services.PlatformResource.GetResource<Color>("PasteToolbarBgColor"));
            SetThemeValue(
                MpThemeResourceKey.ThemePasteButtonDefaultBgColor,
                is_dark ?
                    Mp.Services.PlatformResource.GetResource<Color>("PasteButtonDefaultBgColor_dark") :
                    Mp.Services.PlatformResource.GetResource<Color>("PasteButtonDefaultBgColor"));
            SetThemeValue(
                MpThemeResourceKey.ThemePasteButtonCustomBgColor,
                is_dark ?
                    Mp.Services.PlatformResource.GetResource<Color>("PasteButtonCustomBgColor_dark") :
                    Mp.Services.PlatformResource.GetResource<Color>("PasteButtonCustomBgColor"));

            if(MpAvPrefViewModel.Instance != null) {
                // FONT STUFF
                MpAvPrefViewModel.Instance.OnPropertyChanged(nameof(MpAvPrefViewModel.Instance.IsTextRightToLeft));

                SetThemeValue(MpThemeResourceKey.IsRtl, MpAvPrefViewModel.Instance.IsTextRightToLeft);
                bool test = GetThemeValue<bool>(MpThemeResourceKey.IsRtl);
                IsRtl = MpAvPrefViewModel.Instance.IsTextRightToLeft;

                if (IsRtl &&
                    MpAvPrefViewModel.Instance.DefaultReadOnlyFontFamily == MpAvPrefViewModel.BASELINE_DEFAULT_READ_ONLY_FONT) {
                    // BUG not sure what the cause is but Nunito in Arabic has random problems rendering...
                    // so if font isn't user defined change to avoid
                    MpAvPrefViewModel.Instance.DefaultReadOnlyFontFamily = MpAvPrefViewModel.BASELINE_DEFAULT_READ_ONLY_FONT2;
                }
            }

            MpMessenger.SendGlobal(MpMessageType.ThemeChanged);
        }

        #endregion

        #region Commands
        public ICommand SaveThemeToFileCommand => new MpCommand(
            () => {
                Dispatcher.UIThread.Post(() => {
                    CreatePalette();
                    string theme_css_path = @"C:\Users\tkefauver\Desktop\theme.css";
                    string theme_css_text = GetThemeCss();
                    MpFileIo.WriteTextToFile(theme_css_path, theme_css_text);
                });
            });
        #endregion
    }
}
