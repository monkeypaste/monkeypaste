using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Linq;

namespace MpWpfApp {
    public class MpActionIdToThicknessConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            if (value == null) {
                return new Thickness();
            }
            var avm = MpActionCollectionViewModel.Instance.AllActions.FirstOrDefault(x => x.ActionId == (int)value);
            if (avm == null) {
                return null;
            }
            double indentStep = 10;
            int indentCount = 0;
            var temp = avm;
            while (temp.ParentTreeItem != null) {
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
