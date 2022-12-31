using Avalonia;
using Avalonia.Data.Converters;
using Avalonia.Media.Imaging;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Avalonia;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;

namespace MonkeyPaste.Avalonia {
    public class MpAvStringHexToBitmapTintConverter : IValueConverter {
        public static readonly MpAvStringHexToBitmapTintConverter Instance = new();

        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture) {
            object imgResourceObj = null;
            string hex = null;
            if(parameter is string paramStr) {
                if(paramStr.Contains("|")) {
                    hex = MpSystemColors.ConvertFromString(paramStr.SplitNoEmpty("|")[0]);
                    imgResourceObj = MpSystemColors.ConvertFromString(paramStr.SplitNoEmpty("|")[1]);
                } else if(paramStr.IsStringImageResourcePathOrKey()) {
                    imgResourceObj = paramStr;
                } else {
                    hex = MpSystemColors.ConvertFromString(paramStr, null);
                }
            }

            if(value is int paramIconId) {
                imgResourceObj = paramIconId;
            }
            if(value is string valStr) {
                if (valStr.IsStringImageResourcePathOrKey()) {
                    imgResourceObj = valStr;
                } else {
                    hex = MpSystemColors.ConvertFromString(valStr, null);
                }
            }
            if(hex == null) {
                hex = "#FF000000";
            } 
            if(imgResourceObj == null) {
                imgResourceObj = "TextureImage";
            }
            if(imgResourceObj is string &&
                !imgResourceObj.ToString().IsStringImageResourcePathOrKey()) {
                try {
                    if(int.TryParse(imgResourceObj.ToString(), out int iconId)) {
                        imgResourceObj = iconId;
                    }
                }catch {
                    // what does imgResource end with? what is value and param?
                    Debugger.Break();
                }
            }
            var bmp = MpAvIconSourceObjToBitmapConverter.Instance.Convert(imgResourceObj, null, null, null) as Bitmap;
            bmp = bmp.Tint(hex);
            return bmp;
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) {
            throw new NotSupportedException();
        }
    }

    public class MpAvMultiObjToContrastTintedBitmapConverter : IMultiValueConverter {
        public static readonly MpAvMultiObjToContrastTintedBitmapConverter Instance = new();
        public object Convert(IList<object> values, Type targetType, object parameter, CultureInfo culture) {
            Bitmap bmp = null;
            if(values != null) {
                object bg_hex = values.FirstOrDefault(x => x is string valStr && valStr.IsStringHexColor());
                object base_bmp_resource_obj = values.FirstOrDefault(x => !x.Equals(bg_hex));
                bg_hex = bg_hex == null ? MpSystemColors.Black : bg_hex;

                bmp = MpAvIconSourceObjToBitmapConverter.Instance.Convert(base_bmp_resource_obj, null, null, null) as Bitmap;
                if(bmp == null) {
                    return null;
                }
                if(bg_hex.ToString().IsHexStringBright()) {
                    bmp = bmp.Tint(MpSystemColors.Black);
                } else {
                    bmp = bmp.Tint(MpSystemColors.White);
                }
            }
            return bmp;
        }
    }
}
