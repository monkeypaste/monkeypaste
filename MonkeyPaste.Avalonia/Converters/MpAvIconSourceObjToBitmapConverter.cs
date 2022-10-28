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

namespace MonkeyPaste.Avalonia {
    public class MpAvIconSourceObjToBitmapConverter : IValueConverter {
        public static MpAvIconSourceObjToBitmapConverter Instance = new MpAvIconSourceObjToBitmapConverter();

        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture) {
            double scale = 1.0d;
            List<string> paramParts = parameter == null ? new List<string>() : parameter.ToString().Split(new string[] { "|" }, StringSplitOptions.RemoveEmptyEntries).ToList();
            
            if(paramParts.FirstOrDefault(x=>x.StartsWith("scale_")) is string paramStr) {
                try {
                    scale = double.Parse(paramStr.ToLower().Replace("scale_", String.Empty));
                }catch {
                    scale = 1.0d;
                }
            }

            if (value is int iconId) {
                var ivm = MpAvIconCollectionViewModel.Instance.IconViewModels.FirstOrDefault(x => x.IconId == iconId);

                if (ivm == null) {
                    return null;
                }
                if (paramParts.Contains("border")) {
                    return new MpAvStringBase64ToBitmapConverter().Convert(ivm.IconBorderBase64, null, scale.ToString(), CultureInfo.CurrentCulture);
                }
                return new MpAvStringBase64ToBitmapConverter().Convert(ivm.IconBase64, null, scale.ToString(), CultureInfo.CurrentCulture);
            } else if(value is string valStr) {
                //types: resource key, hex color, base64
                if(valStr.EndsWith("Image") || valStr.IsAvResourceString()) {
                    return new MpAvStringResourceConverter()
                                .Convert(value, targetType, parameter, culture) as Bitmap;
                }
                if(valStr.EndsWith("Svg")) {
                    if(Application.Current.Resources.TryGetValue(valStr, out object data)) {
                        return data as Geometry;
                    }

                }
                if(valStr.IsStringBase64()) {
                    return new MpAvStringBase64ToBitmapConverter().Convert(valStr, null, null, CultureInfo.CurrentCulture);
                }
            } 
            return null;

        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotSupportedException();
        }
    }
}
