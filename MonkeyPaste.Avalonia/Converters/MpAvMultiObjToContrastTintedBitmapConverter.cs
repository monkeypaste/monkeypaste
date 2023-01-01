using Avalonia.Data.Converters;
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
            if(values != null) {
                object bg_hex = values.FirstOrDefault(x => x is string valStr && valStr.IsStringHexColor());
                object base_bmp_resource_obj = values.Where(x=>x is string || x is int).FirstOrDefault(x => !x.Equals(bg_hex));
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
