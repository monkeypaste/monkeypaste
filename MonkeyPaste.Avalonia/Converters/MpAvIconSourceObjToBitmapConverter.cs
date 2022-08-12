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

namespace MonkeyPaste.Avalonia {
    public class MpAvIconSourceObjToBitmapConverter : IValueConverter {
        public static MpAvIconSourceObjToBitmapConverter Instance = new MpAvIconSourceObjToBitmapConverter();

        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture) {
            double scale = 1.0d;
            if(parameter is string paramStr &&
                paramStr.ToLower().StartsWith("scale_")) {
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
                if (parameter is string paramStr2) {
                    if (paramStr2.ToLower() == "border") {
                        return new MpAvStringBase64ToBitmapConverter().Convert(ivm.IconBorderBase64, null, scale.ToString(), CultureInfo.CurrentCulture);
                    }
                }
                return new MpAvStringBase64ToBitmapConverter().Convert(ivm.IconBase64, null, scale.ToString(), CultureInfo.CurrentCulture);
            } else if(value is string valStr) {
                //types: resource key, hex color, base64
                if(valStr.EndsWith("Image") || valStr.IsAvResourceString()) {
                    return new MpAvStringResourceToBitmapConverter()
                                .Convert(value, targetType, parameter, culture) as Bitmap;
                }
                if(valStr.EndsWith("Svg")) {
                    if(Application.Current.Resources.TryGetValue(valStr, out object data)) {
                        return data as Geometry;
                    }

                }
            } 
            return null;

        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotSupportedException();
        }
    }
}
