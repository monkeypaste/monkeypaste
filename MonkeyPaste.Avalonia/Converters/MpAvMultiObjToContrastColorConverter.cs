using Avalonia.Data.Converters;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Avalonia;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace MonkeyPaste.Avalonia {
    public class MpAvMultiObjToContrastColorConverter : IMultiValueConverter {
        public static readonly MpAvMultiObjToContrastColorConverter Instance = new();
        public object Convert(IList<object> values, Type targetType, object parameter, CultureInfo culture) {
            Color result = Colors.Transparent;
            if (values != null &&
                values.All(x => !x.IsUnsetValue())) {
                string base_bg_hex = values[0] as string;
                string fg_hex = null;
                if (values[1] is int iconId &&
                    MpAvIconCollectionViewModel.Instance.IconViewModels.FirstOrDefault(x => x.IconId == iconId) is MpAvIconViewModel ivm) {
                    // use icon primary color for brightness
                    if (ivm.PrimaryIconColorList.FirstOrDefault() is string primaryColor) {
                        fg_hex = primaryColor;
                    } else {
                        MpDebug.Break("Icon w/o border error");
                    }

                } else if (values[1] is string icon_str &&
                            icon_str.IsStringHexColor()) {
                    // ignore default icons (string ending in 'Image') they're all black
                    // case for classify tag color
                    fg_hex = icon_str;
                }

                if (fg_hex.IsStringHexColor()) {
                    if (MpColorHelpers.IsBright(fg_hex)) {
                        result = MpColorHelpers.GetDarkerHexColor(base_bg_hex).ToAvColor();
                    } else {
                        result = MpColorHelpers.GetLighterHexColor(base_bg_hex).ToAvColor();
                    }
                } else {
                    result = base_bg_hex.ToAvColor();
                }

            }

            return new SolidColorBrush(result);
        }
    }
}
