using Avalonia;
using Avalonia.Data.Converters;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace MonkeyPaste.Avalonia {
    public class MpAvTagIdToThicknessConverter : IMultiValueConverter {
        public static readonly MpAvTagIdToThicknessConverter Instance = new();


        public object Convert(IList<object> values, Type targetType, object parameter, CultureInfo culture) {
            if (values == null || values.Count < 2 || (bool)values[1] == false) {
                return new Thickness();
            }
            var ttvm = MpAvTagTrayViewModel.Instance.Items.FirstOrDefault(x => x.TagId == (int)values[0]);
            if (ttvm == null) {
                return null;
            }
            int indentCount = 0;
            var temp = ttvm;
            while (temp.ParentTreeItem != null) {
                indentCount++;
                temp = temp.ParentTreeItem;
            }
            double indentSize = (double)values[2] * indentCount;
            return new Thickness(indentSize, 0, 0, 0);
        }
        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) {
            throw new NotSupportedException();
        }

    }
}
