using Avalonia.Controls;
using Avalonia.Data.Converters;
using Avalonia.Media.Imaging;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Avalonia;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace MonkeyPaste.Avalonia {
    public class MpAvStringHexToBitmapTintConverter : IValueConverter {
        private Dictionary<object, Dictionary<string, Bitmap>> _tintCache = new Dictionary<object, Dictionary<string, Bitmap>>();

        private bool IS_DYNAMIC_TINT_ENABLED = true;

        public static readonly MpAvStringHexToBitmapTintConverter Instance = new();

        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture) {
            if (!IS_DYNAMIC_TINT_ENABLED) {
                return MpAvIconSourceObjToBitmapConverter.Instance.Convert(value, targetType, parameter, culture);
            }

            if (MpAvThemeViewModel.Instance.IsColoredImageResource(value)) {
                // ignore tinting known color images or db images (since colors unknown)
                return MpAvIconSourceObjToBitmapConverter.Instance.Convert(value, targetType, parameter, culture);
            }

            object imgResourceObj = null;
            string hex = null;
            if (parameter is string paramStr &&
                paramStr.SplitNoEmpty("|") is string[] paramParts) {
                if (paramParts.FirstOrDefault(x => x.StartsWith("Theme")) is string theme_key &&
                    Enum.TryParse(theme_key, true, out MpThemeResourceKey trk)) {
                    hex = Mp.Services.PlatformResource.GetResource<string>(trk.ToString());
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
                    // what does imgResource end with? what is value and param?
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

        public void RefreshCache() {
            _tintCache.Clear();

            MpAvWindowManager.AllWindows
                    .SelectMany(x => x.GetVisualDescendants<Image>())
                    .ForEach(x => x.InvalidateVisual());
        }
    }
}
