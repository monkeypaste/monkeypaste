using Avalonia;
using Avalonia.Data.Converters;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace MonkeyPaste.Avalonia {
    public class MpAvTagIdToTagTileViewModelConverter : IValueConverter {
        public static readonly MpAvTagIdToTagTileViewModelConverter Instance = new();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            if(value is int tagId) {
                return MpAvTagTrayViewModel.Instance.Items.FirstOrDefault(x => x.TagId == tagId);
            }
            return null;
        }
        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) {
            if(value is MpAvTagTileViewModel ttvm) {
                return ttvm.TagId;
            }
            return 0;
        }

    }
}
