using Avalonia.Data.Converters;
using System;
using System.Globalization;
using System.Linq;

namespace MonkeyPaste.Avalonia {
    public class MpAvAnalyticItemGuidToIconSourceConverter : IValueConverter {
        public static readonly MpAvAnalyticItemGuidToIconSourceConverter Instance = new();

        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture) {
            var aivm = MpAvAnalyticItemCollectionViewModel.Instance.Items.FirstOrDefault(x => x.PluginGuid == value.ToString());
            if (aivm == null) {
                return null;
            }
            return new MpAvIconSourceObjToBitmapConverter().Convert(aivm.PluginIconId, targetType, parameter, culture);
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) {
            throw new NotSupportedException();
        }
    }
}
