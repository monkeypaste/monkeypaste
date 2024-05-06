using Avalonia.Controls;
using Avalonia.Data.Converters;
using Avalonia.Media.Imaging;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Avalonia;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace MonkeyPaste.Avalonia {
    public class MpAvStringHexToBitmapTintConverter : IValueConverter {
        #region Statics
        static MpAvStringHexToBitmapTintConverter() {
            MpMessenger.RegisterGlobal(ReceivedGlobalMessage);
        }
        private static void ReceivedGlobalMessage(MpMessageType msg) {
            switch (msg) {
                case MpMessageType.ThemeChanged:
                    RefreshCache();
                    break;
            }
        }

        public static void RefreshCache() {
            _tintCache.Clear();

            MpAvWindowManager.AllWindows
                    .SelectMany(x => x.GetVisualDescendants<Image>())
                    .ForEach(x => x.Redraw());
        }

        #endregion
        private static Dictionary<object, Dictionary<string, Bitmap>> _tintCache { get; set; } = new Dictionary<object, Dictionary<string, Bitmap>>();

        public static bool IS_DYNAMIC_TINT_ENABLED =
#if ANDROID
        false;
#else            
        true;
#endif

        public static readonly MpAvStringHexToBitmapTintConverter Instance = new();

        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture) {
            if (!IS_DYNAMIC_TINT_ENABLED) {
                return MpAvIconSourceObjToBitmapConverter.Instance.Convert(value, targetType, parameter, culture);
            }
            if (parameter.ToStringOrEmpty() == "test") {

            }

            object imgResourceObj = null;
            string hex = null;
            if (value is object[] valParts) {
                hex = valParts.FirstOrDefault(x => x.ToStringOrEmpty().IsStringHexOrNamedColor()).ToStringOrDefault();
                imgResourceObj = valParts.FirstOrDefault(x => x.ToStringOrDefault() != hex);
                //return Convert(imgResourceObj, null, new[] { "force", hex }, null);
                return Convert(imgResourceObj, targetType, hex, culture);
            }
            bool ignore_color_imgs = true;
            if (parameter is object[] paramParts2 &&
                paramParts2[0].ToStringOrEmpty() == "force") {
                ignore_color_imgs = false;
                parameter = paramParts2[1];
            }

            if (MpAvThemeViewModel.Instance.IsColoredImageResource(value) &&
                ignore_color_imgs) {
                // ignore tinting known color images or db images (since colors unknown)
                return MpAvIconSourceObjToBitmapConverter.Instance.Convert(value, targetType, parameter, culture);
            }


            if (parameter is string paramStr &&
                paramStr.SplitNoEmpty("|") is string[] paramParts) {
                if (paramParts.FirstOrDefault(x => x.StartsWith("Theme")) is string theme_key &&
                    Enum.TryParse(theme_key, true, out MpThemeResourceKey trk)) {
                    if (trk == MpThemeResourceKey.ThemeInteractiveColor &&
                        value.ToStringOrEmpty() is string valueStr) {
                        // when image
                        // NOTE only using top row w/o last column
                        int max = MpSystemColors.COLOR_PALETTE_COLS - 2;
                        int randColorSeed = valueStr.Length;
                        if (valueStr.IsStringImageResourceKey() &&
                            Mp.Services.PlatformResource.GetResource<string>(valueStr) is string res_path) {
                            // when paramValue is key add its resource path's length to vary color more
                            randColorSeed += res_path.Length;
                        }
                        int rand_idx = (randColorSeed % max) * MpSystemColors.COLOR_PALETTE_ROWS;
                        hex = MpSystemColors.ContentColors[rand_idx].RemoveHexAlpha();
                        if (MpAvThemeViewModel.Instance.IsThemeDark) {
                            hex = MpColorHelpers.MakeBright(hex);
                        } else {
                            hex = MpColorHelpers.MakeDark(hex);
                        }
                    } else if (trk == MpThemeResourceKey.ThemeInteractiveColor_norand) {
                        // only used by cap overlay currently
                        hex = Mp.Services.PlatformResource.GetResource<string>(MpThemeResourceKey.ThemeInteractiveColor.ToString());
                    } else {
                        hex = Mp.Services.PlatformResource.GetResource<string>(trk.ToString());
                    }
                    if (!string.IsNullOrEmpty(hex)) {
                        if (paramParts.Any(x => x == "darker")) {
                            hex = MpColorHelpers.GetDarkerHexColor(hex);
                        } else if (paramParts.Any(x => x == "lighter")) {
                            hex = MpColorHelpers.GetLighterHexColor(hex);
                        }
                    }


                } else if (paramParts.Length > 1) {
                    hex = MpColorHelpers.ParseHexFromString(paramParts[0]);
                } else if (paramStr.IsStringImageResourcePathOrKey()) {
                    imgResourceObj = paramStr;
                } else {
                    hex = MpColorHelpers.ParseHexFromString(paramStr, null);
                }
            }

            if (value is int paramIconId) {
                imgResourceObj = paramIconId;
            }
            if (value is string valStr) {
                if (valStr.IsStringImageResourcePathOrKey()) {
                    imgResourceObj = valStr;
                } else {
                    hex = MpColorHelpers.ParseHexFromString(valStr, null);
                }
            }
            if (hex == null) {
                hex = "#FF000000";
            }
            if (imgResourceObj == null) {
                imgResourceObj = "TextureImage";
            }
            if (imgResourceObj is string &&
                !imgResourceObj.ToString().IsStringImageResourcePathOrKey()) {
                try {
                    if (int.TryParse(imgResourceObj.ToString(), out int iconId)) {
                        imgResourceObj = iconId;
                    }
                }
                catch {
                    // what does imgResource end with? what is paramValue and param?
                    MpDebug.Break();
                }
            }

            Bitmap bmp = null;
            if (_tintCache.TryGetValue(imgResourceObj, out var tintLookup)) {
                tintLookup.TryGetValue(hex, out bmp);
            }
            if (bmp == null) {
                bmp = MpAvIconSourceObjToBitmapConverter.Instance.Convert(imgResourceObj, null, null, null) as Bitmap;
                bmp = bmp.Tint(hex);
                if (bmp != null) {
                    if (!_tintCache.ContainsKey(imgResourceObj)) {
                        _tintCache.Add(imgResourceObj, new Dictionary<string, Bitmap>());
                    }
                    _tintCache[imgResourceObj].Add(hex, bmp);
                }
            }

            return bmp;
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) {
            throw new NotSupportedException();
        }


        public static string GetTintCacheLog() {
            var sb = new StringBuilder();
            _tintCache.ForEach(x => sb.AppendLine(x.Key.ToString()));
            return sb.ToString();
        }

    }
}
