using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Threading;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Avalonia;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

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
        MpAvViewModelBase,
        MpICloseWindowViewModel {

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
            "add.png",
            "appstore.png",
            "banana.png",
            "bitmap.png",
            "bingicon.png",
            "clipboard.png",
            "close.png",
            "cogcolor.png",
            "colors.png",
            "delete.png",
            "delete2.png",
            "device.png",
            "duckduckgoicon.png",
            "error.png",
            "folder.png",
            "gavel.png",
            "ghost.png",
            "global.png",
            "graph.png",
            "html.png",
            "info.png",
            "jigsaw.png",
            "joystickative.png",
            "joystickative2.png",
            "monkey.png",
            "monkeywink.png",
            "noentry.png",
            "private.png",
            "pindown.png",
            "pindownover.png",
            "pinover.png",
            "preferences.png",
            "questionmark.png",
            "remove.png",
            "rtf.png",
            "sheep.png",
            "spellcheck.png",
            "stargold.png",
            "staryellow.png",
            "staryellow2.png",
            "text.png",
            "trophy.png",
            "warning.png",
            "warningtime.png",
            "yandexicon.png",
        };


        #endregion

        #region Constants
        public const double PHI = 1.618033988749894d;
        #endregion

        #region Statics
        public static bool IS_WINDOW_FADE_ENABLED = true;

        private static MpAvThemeViewModel _instance;
        public static MpAvThemeViewModel Instance => _instance ?? (_instance = new MpAvThemeViewModel());

        public void Init() {
            // empty
        }
        #endregion

        #region Interfaces

        #region MpIChildWindowViewModel Implementation

        public bool IsWindowOpen { get; set; }
        public MpWindowType WindowType =>
            MpWindowType.Modal;

        #endregion
        #endregion

        #region Properties

        #region Appearance

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

        public int ShakeDurMs =>
            500;

        public int FocusPulseDurMs =>
            3_000;
        public string[] CustomFontFamilyNames => new string[] {
                "Habanero",
                "Nunito"
            };
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
            UpdateThemeResources();
        }
        #endregion

        #region Public Methods
        public void UpdateThemeResources(bool showWait = false) {
            _themePrefPropNames.ForEach(x => SyncThemePref(x));

            if (showWait) {
                ShowWaitWindow();
            }
            CreatePalette();
            if (showWait) {
                IsWindowOpen = false;
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
                res_str = Mp.Services.PlatformResource.GetResource<string>(res_str);
            }
            return _colorImageFileNames.Any(x => res_str.ToLower().EndsWith(x));
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
            this.SetPropertyValue(prefName, MpAvPrefViewModel.Instance.GetPropertyValue(prefName));
        }

        private T GetThemeValue<T>(MpThemeResourceKey trk) {
            return (T)Mp.Services.PlatformResource.GetResource(trk.ToString());
        }

        private void SetThemeValue(MpThemeResourceKey trk, object value) {
            Mp.Services.PlatformResource.SetResource(trk.ToString(), value);

        }
        private void ShowWaitWindow() {
            var w = new MpAvWindow() {
                Width = 350,
                Height = 300,
                SystemDecorations = SystemDecorations.None,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                DataContext = this,
                Content = new StackPanel() {
                    Orientation = Orientation.Vertical
                }
            };
            if (w.Content is StackPanel sp) {
                sp.Children.Add(new TextBlock() {
                    HorizontalAlignment = HorizontalAlignment.Stretch,
                    VerticalAlignment = VerticalAlignment.Center,
                    Height = 125,
                    TextAlignment = TextAlignment.Center,
                    Margin = new Thickness(0, 0, 0, 15),
                    Text = "Please Wait..."
                });
                sp.Children.Add(new MpAvBusySpinnerView() {
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                    Width = 70,
                    Height = 70
                });
            }

            w.ShowChild(MpAvWindowManager.LocateWindow(MpAvSettingsViewModel.Instance));
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

            var tt = MpAvPrefViewModel.Instance.ThemeType;
            string hex = MpAvPrefViewModel.Instance.ThemeColor;
            // prepass selected color to get decent chroma
            // V >= 50, S >= 50
            hex.ToPortableColor().ColorToHsv(out double preh, out double pres, out double prev);
            //pres = Math.Min(0.9d, Math.Max(0.5d, pres));
            prev = Math.Max(0.5d, prev);
            hex = MpColorHelpers.ColorFromHsv(preh, pres, prev).ToHex(true);

            if (tt == MpThemeType.Dark) {
                hex.ToPortableColor().ColorToHsl(out double th, out double ts, out double tl);
                //if (tt == MpThemeType.Dark) {
                tl = Math.Max(25d / 100d, tl - (15d / 100d));
                //} else {
                //    tl = Math.Min(75d / 100d, tl + (10d / 100d));
                //}
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
            string dark_gray = MpSystemColors.dimgray.RemoveHexAlpha(); //MpColorHelpers.ColorFromHsv(h, 0.05d, Math.Max(0, v - 0.3d)).ToHex(true);
            string med_gray = MpSystemColors.gray.RemoveHexAlpha(); //MpColorHelpers.ColorFromHsv(h, 0.05d, Math.Max(0, v - 0.1d)).ToHex(true);
            string light_gray = MpSystemColors.lightgray.RemoveHexAlpha(); //MpColorHelpers.ColorFromHsv(h, 0.1d, Math.Max(0, v - 0.1d)).ToHex(true);
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
            palette.Add(MpColorHelpers.ColorFromHsv(h, 0.05d, 0.9d).ToHex(true));

            if (tt == MpThemeType.Dark) {
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

            var colors = palette.Select(x => x.ToAvColor()).ToArray();
            SetThemeValue(MpThemeResourceKey.ThemeColor, colors[0]);
            SetThemeValue(MpThemeResourceKey.ThemeAccent1BgColor, colors[1]);
            SetThemeValue(MpThemeResourceKey.ThemeAccent5BgColor, colors[2]);
            SetThemeValue(MpThemeResourceKey.ThemeAccent5Color, colors[3]);
            SetThemeValue(MpThemeResourceKey.ThemeAccent3Color, colors[4]);
            SetThemeValue(MpThemeResourceKey.ThemeAccent2Color, colors[5]);
            SetThemeValue(MpThemeResourceKey.ThemeAccent1Color, colors[6]);
            SetThemeValue(MpThemeResourceKey.ThemeCompliment1Color, colors[7]);
            SetThemeValue(MpThemeResourceKey.ThemeCompliment1DarkColor, colors[8]);

            SetThemeValue(MpThemeResourceKey.ThemeGrayAccent1Color, colors[9]);
            SetThemeValue(MpThemeResourceKey.ThemeGrayAccent2Color, colors[10]);
            SetThemeValue(MpThemeResourceKey.ThemeGrayAccent3Color, colors[11]);

            SetThemeValue(MpThemeResourceKey.ThemeBlackColor, colors[12]);
            SetThemeValue(MpThemeResourceKey.ThemeWhiteColor, colors[13]);

            SetThemeValue(MpThemeResourceKey.ThemeAccent4Color, colors[14]);
            SetThemeValue(MpThemeResourceKey.ThemeAccent4BgColor, colors[15]);

            SetThemeValue(MpThemeResourceKey.ThemeCompliment2Color, colors[16]);
            SetThemeValue(MpThemeResourceKey.ThemeCompliment2DarkColor, colors[17]);

            SetThemeValue(MpThemeResourceKey.ThemeCompliment3Color, colors[18]);
            SetThemeValue(MpThemeResourceKey.ThemeCompliment3DarkColor, colors[19]);

            SetThemeValue(MpThemeResourceKey.ThemeInteractiveColor, colors[20]);
            SetThemeValue(MpThemeResourceKey.ThemeInteractiveBgColor, colors[21]);

            SetThemeValue(MpThemeResourceKey.ThemeCompliment4Color, colors[22]);
            SetThemeValue(MpThemeResourceKey.ThemeCompliment4DarkColor, colors[23]);

            SetThemeValue(MpThemeResourceKey.ThemeCompliment5Color, colors[24]);
            SetThemeValue(MpThemeResourceKey.ThemeCompliment5DarkColor, colors[25]);
            SetThemeValue(MpThemeResourceKey.ThemeCompliment5LighterColor, colors[26]);

            var test = palette.Where(x => x.ToLower() == "#ff252226" || x.ToLower() == "#252226").Select(x => palette.IndexOf(x));


            MpMessenger.SendGlobal(MpMessageType.ThemeChanged);
        }

        #endregion

        #region Commands
        #endregion
    }
}
