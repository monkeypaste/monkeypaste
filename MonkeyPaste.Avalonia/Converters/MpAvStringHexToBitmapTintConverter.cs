using Avalonia;
using Avalonia.Controls;
using Avalonia.Data.Converters;
using Avalonia.Media.Imaging;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Avalonia;
using System;
using System.Diagnostics;
using System.Globalization;

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
}
