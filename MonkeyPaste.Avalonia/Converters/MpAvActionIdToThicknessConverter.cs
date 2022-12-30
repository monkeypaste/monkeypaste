using Avalonia;
using Avalonia.Data.Converters;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace MonkeyPaste.Avalonia {
    public class MpAvActionIdToThicknessConverter : IValueConverter {
        public static readonly MpAvActionIdToThicknessConverter Instance = new();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            if (value == null) {
                return new Thickness();
            }
            var avm = MpAvTriggerCollectionViewModel.Instance.AllActions.FirstOrDefault(x => x.ActionId == (int)value);
            if (avm == null) {
                return null;
            }
            double indentStep = 10;
            int indentCount = 0;
            var temp = avm;
            while (temp.ParentActionViewModel != null) {
                indentCount++;
                temp = temp.ParentActionViewModel;
            }
            //for first indent level indent icon is all that is needed
            indentCount--;
            indentCount = Math.Max(0, indentCount);
            double indentSize = indentStep * indentCount;
            return new Thickness(indentSize, 0, 0, 0);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }
    }
}
