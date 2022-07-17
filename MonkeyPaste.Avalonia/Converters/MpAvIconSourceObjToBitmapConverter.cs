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

namespace MonkeyPaste.Avalonia {
    public class MpAvIconSourceObjToBitmapConverter : IValueConverter {
        public static MpAvIconSourceObjToBitmapConverter Instance = new MpAvIconSourceObjToBitmapConverter();

        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture) {
            if (value is int iconId) {
                //var ivm = MpIconCollectionViewModel.Instance.IconViewModels.FirstOrDefault(x => x.IconId == iconId);

                //if (ivm == null) {
                //    return null;
                //}
                //if (parameter is string paramStr) {
                //    if (paramStr.ToLower() == "border") {
                //        return new MpBase64StringToBitmapSourceConverter().Convert(ivm.IconBorderImage.ImageBase64, null, null, CultureInfo.CurrentCulture);
                //    }
                //}
                //return new MpBase64StringToBitmapSourceConverter().Convert(ivm.IconImage.ImageBase64, null, null, CultureInfo.CurrentCulture);
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
