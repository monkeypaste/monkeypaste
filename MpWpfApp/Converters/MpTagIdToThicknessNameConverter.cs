using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace MpWpfApp {
    public class MpTagIdToThicknessNameConverter : IMultiValueConverter {
        
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture) {
            if(values == null || values.Length < 2 || (bool)values[1] == false) {
                return new Thickness();
            }
            var ttvm = MpTagTrayViewModel.Instance.Items.FirstOrDefault(x => x.TagId == (int)values[0]);
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
        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }
    }
}
