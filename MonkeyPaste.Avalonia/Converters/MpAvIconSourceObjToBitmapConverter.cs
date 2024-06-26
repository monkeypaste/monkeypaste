﻿using Avalonia.Controls;
using Avalonia.Data.Converters;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Avalonia;
using MonkeyPaste.Common.Plugin;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace MonkeyPaste.Avalonia {
    public class MpAvIconSourceObjToBitmapConverter : IValueConverter {
        Bitmap _questionMarkBmp;
        Bitmap _fallback =>
            _questionMarkBmp ?? (_questionMarkBmp = MpAvStringBase64ToBitmapConverter.Instance.Convert(MpBase64Images.QuestionMark, null, 1.0, CultureInfo.CurrentCulture) as Bitmap);

        public static MpAvIconSourceObjToBitmapConverter Instance = new MpAvIconSourceObjToBitmapConverter();

        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture) {
            if (parameter.ToStringOrEmpty() == "test") {

            }
            if (value is int blah && blah == 11) {

            }
            double scale = 1.0d;
            List<string> paramParts = parameter == null ? new List<string>() : parameter.ToString().SplitNoEmpty("|").ToList();

            if (value is int iconId) {
                var ivm = MpAvIconCollectionViewModel.Instance.IconViewModels.FirstOrDefault(x => x.IconId == iconId);

                if (ivm == null) {
                    return _fallback;
                }
                if (paramParts.Contains("border")) {
                    string border_tint = paramParts.FirstOrDefault(x => x != "border");
                    return new MpAvStringBase64ToBitmapConverter().Convert(ivm.IconBorderBase64, null, border_tint, CultureInfo.CurrentCulture);
                }
                return new MpAvStringBase64ToBitmapConverter().Convert(ivm.IconBase64, null, scale.ToString(), CultureInfo.CurrentCulture);
            }

            if (value is not string valStr) {
                return _fallback;
            }

            bool is_file_or_dir = valStr.IsFileOrDirectory();
            bool wants_path_icon = paramParts.Any(x => x.ToLowerInvariant() == "pathimg");
            if (is_file_or_dir || wants_path_icon) {
                if (valStr.IsSupportedImageFileType() && !wants_path_icon) {
                    try {
                        using (var fs = new FileStream(valStr, FileMode.Open)) {
                            return new Bitmap(fs);
                        }
                    }
                    catch (Exception ex) {
                        MpConsole.WriteTraceLine($"Error convert img path to bitmap. Path: '{valStr}'", ex);
                        return null;
                    }
                }
                string appIconBase64 = is_file_or_dir ?
                    Mp.Services.IconBuilder.GetPathIconBase64(valStr, wants_path_icon ? MpIconSize.SmallIcon16 : MpIconSize.MediumIcon32) :
                    MpAvPrefViewModel.Instance.ThemeType == MpThemeType.Dark ?
                        MpBase64Images.MissingFile_white :
                        MpBase64Images.MissingFile;
                if (targetType == typeof(string)) {
                    return appIconBase64;
                }
                return new MpAvStringBase64ToBitmapConverter().Convert(appIconBase64, null, null, CultureInfo.CurrentCulture);
            }
            //types: resource key, hex color, base64, file system path, shape name
            var valParts = valStr.SplitNoEmpty(",");

            string hex_color = valParts.FirstOrDefault(x => x.IsStringHexColor());
            if (string.IsNullOrEmpty(hex_color)) {
                string named_color = valParts.FirstOrDefault(x => x.IsStringNamedColor());
                if (!string.IsNullOrEmpty(named_color)) {
                    hex_color = MpColorHelpers.ParseHexFromString(named_color);
                }
            }
            if (!string.IsNullOrEmpty(hex_color)) {
                string color_img_key = "RoundedTextureImage";
                if (valParts.Length > 1) {
                    color_img_key = valParts.FirstOrDefault(x => x.Contains("Image"));
                }
                //var blank_bmp = MpAvStringResourceConverter.Instance.Convert(
                //    Mp.Services.PlatformResource.GetResource(color_img_key), null, null, null) as Bitmap;
                //blank_bmp = blank_bmp.Tint(valStr);
                var colored_blank_bmp = MpAvStringHexToBitmapTintConverter.Instance.Convert(
                    Mp.Services.PlatformResource.GetResource(color_img_key), null, hex_color, null) as Bitmap;
                return colored_blank_bmp;
            }

            if (valParts.Length > 1) {
                // should only have parts for color resource
                MpDebug.Break();
            }
            if (valStr.EndsWith("Icon") || targetType == typeof(MpAvWindowIcon) || targetType == typeof(WindowIcon)) {
                string res_uri = valStr.IsAvResourceString() ? valStr : Mp.Services.PlatformResource.GetResource<string>(valStr);
                using (var icon_stream = AssetLoader.Open(new Uri(res_uri))) {
#if WINDOWS
                    if (res_uri.EndsWith("png") && OperatingSystem.IsWindows()) {
                        // avoid exception using png
                        using (var conv_stream = new MemoryStream()) {
                            if (MpPngToIcoConverter.ConvertToIcon(icon_stream, conv_stream)) {
                                if(targetType == typeof(WindowIcon)) {
                                    return new WindowIcon(conv_stream);
                                }
                                return new MpAvWindowIcon(conv_stream);
                            }
                        }
                    }
#endif
                    if (targetType == typeof(WindowIcon)) {
                        return new WindowIcon(icon_stream);
                    }
                    return new MpAvWindowIcon(icon_stream);
                }
            }
            if (valStr.EndsWith("Image") || valStr.IsAvResourceString()) {
                return new MpAvStringResourceConverter()
                            .Convert(value, targetType, parameter, culture) as Bitmap;
            }
            if (valStr.EndsWith("Svg")) {
                if (Mp.Services.PlatformResource.GetResource(valStr) is object data) { //}, out var data)) {
                    if (data is Geometry geometry) {
                        return geometry;
                    }
                    if (data is string svgPath) {
                        MpDebug.Break();
                    }
                }
            }
            if (valStr.IsStringBase64()) {
                return new MpAvStringBase64ToBitmapConverter().Convert(valStr, null, null, CultureInfo.CurrentCulture);
            }
            return _fallback;

        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotSupportedException();
        }
    }
}
