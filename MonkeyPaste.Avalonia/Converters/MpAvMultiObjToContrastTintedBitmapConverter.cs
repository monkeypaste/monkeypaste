﻿using Avalonia.Data.Converters;
using Avalonia.Media.Imaging;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Avalonia;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace MonkeyPaste.Avalonia {
    public class MpAvMultiObjToContrastTintedBitmapConverter : IMultiValueConverter {
        public static readonly MpAvMultiObjToContrastTintedBitmapConverter Instance = new();
        public object Convert(IList<object> values, Type targetType, object parameter, CultureInfo culture) {
            Bitmap bmp = null;
            if (values != null) {
                object bg_hex_obj = values.FirstOrDefault(x => x is string valStr && (valStr.IsStringHexColor() || valStr.IsStringNamedColor()));
                string bg_hex_str = null;
                if (bg_hex_obj != null && bg_hex_obj.ToString().IsStringNamedColor()) {
                    bg_hex_str = MpColorHelpers.ParseHexFromString(bg_hex_obj.ToString());
                } else if (bg_hex_obj != null) {
                    bg_hex_str = bg_hex_obj.ToString();
                }
                object base_bmp_resource_obj = values.Where(x => x is string || x is int).FirstOrDefault(x => !x.Equals(bg_hex_obj));
                bg_hex_str = bg_hex_str == null ? MpSystemColors.Black : bg_hex_str;


                string tint_hex = null;
                if (bg_hex_str.IsHexStringTransparent()) {
                    bmp = MpAvIconSourceObjToBitmapConverter.Instance.Convert(base_bmp_resource_obj, null, null, null) as Bitmap;
                } else {
                    tint_hex = bg_hex_str.ToContrastForegoundColor();
                    bmp = MpAvStringHexToBitmapTintConverter.Instance.Convert(base_bmp_resource_obj, null, tint_hex, null) as Bitmap;
                }
            }
            return bmp;
        }
    }
}
