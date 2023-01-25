using System;
using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Platform;
using Avalonia;
using System.Reflection;
using Avalonia.Media.Imaging;
using Avalonia.Media;
using Avalonia.Controls.Shapes;
using MonkeyPaste.Common.Avalonia;
using System.Linq;
using System.Collections.Generic;
using MonkeyPaste.Common;
using System.IO;
using Avalonia.Controls;
using System.Diagnostics;

namespace MonkeyPaste.Avalonia {
    public class MpAvIconSourceObjToBitmapConverter : IValueConverter {
        public static MpAvIconSourceObjToBitmapConverter Instance = new MpAvIconSourceObjToBitmapConverter();

        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture) {
            double scale = 1.0d;
            List<string> paramParts = parameter == null ? new List<string>() : parameter.ToString().Split(new string[] { "|" }, StringSplitOptions.RemoveEmptyEntries).ToList();
            bool isFilePathIcon = paramParts.Any(x => x.ToLower() == "pathicon");

            if (value is int iconId) {
                var ivm = MpAvIconCollectionViewModel.Instance.IconViewModels.FirstOrDefault(x => x.IconId == iconId);

                if (ivm == null) {
                    return null;
                }
                if (paramParts.Contains("border")) {
                    return new MpAvStringBase64ToBitmapConverter().Convert(ivm.IconBorderBase64, null, scale.ToString(), CultureInfo.CurrentCulture);
                }
                return new MpAvStringBase64ToBitmapConverter().Convert(ivm.IconBase64, null, scale.ToString(), CultureInfo.CurrentCulture);
            } 
            
            if(value is string valStr) {
                //types: resource key, hex color, base64, file system path, shape name
                var valParts = valStr.SplitNoEmpty(",");

                string hex_color = valParts.FirstOrDefault(x=>x.IsStringHexColor());
                if(string.IsNullOrEmpty(hex_color)) {
                    string named_color = valParts.FirstOrDefault(x => x.IsStringNamedColor());
                    if(!string.IsNullOrEmpty(named_color)) {
                        hex_color = MpSystemColors.ConvertFromString(named_color);
                    }
                }
                if (!string.IsNullOrEmpty(hex_color)) {
                    string color_img_key = "TextureImage";
                    if(valParts.Length > 1) {
                        color_img_key = valParts.FirstOrDefault(x => x.Contains("Image"));
                    }
                    var blank_bmp = MpAvStringResourceConverter.Instance.Convert(
                        MpPlatformWrapper.Services.PlatformResource.GetResource(color_img_key), null, null, null) as Bitmap;
                    blank_bmp = blank_bmp.Tint(valStr);
                    return blank_bmp;
                }

                if(valParts.Length > 1) {
                    // should only have parts for color resource
                    Debugger.Break();
                }
                if (valStr.EndsWith("Icon")) {
                    return new WindowIcon(
                        MpAvStringResourceConverter.Instance.Convert(
                            MpPlatformWrapper.Services.PlatformResource.GetResource(valStr), null, null, null) as Bitmap);
                }
                if (valStr.EndsWith("Image") || valStr.IsAvResourceString()) {
                    return new MpAvStringResourceConverter()
                                .Convert(value, targetType, parameter, culture) as Bitmap;
                }
                if(valStr.EndsWith("Svg")) {
                    if(Application.Current.Resources.TryGetValue(valStr, out object data)) {
                        if(data is Geometry geometry) {
                            return geometry;
                        }
                        if(data is string svgPath) {
                            Debugger.Break();
                        }
                    }

                }
                if(valStr.IsStringBase64()) {
                    return new MpAvStringBase64ToBitmapConverter().Convert(valStr, null, null, CultureInfo.CurrentCulture);
                }
                if(valStr.IsFile()) {
                    if(valStr.IsKnownImageFile() && !isFilePathIcon) {
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
                    string appIconBase64 = MpPlatformWrapper.Services.IconBuilder.GetApplicationIconBase64(valStr);
                    return new MpAvStringBase64ToBitmapConverter().Convert(appIconBase64, null, null, CultureInfo.CurrentCulture);
                }
            } 
            return null;

        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotSupportedException();
        }
    }
}
