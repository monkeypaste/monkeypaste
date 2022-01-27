using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Linq;

namespace MpWpfApp {
    public class MpTagIdToThicknessNameConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            var ttvm = MpTagTrayViewModel.Instance.Items.FirstOrDefault(x => x.TagId == (int)value);
            if (ttvm == null) {
                return null;
            }
            int indentCount = 0;
            var temp = ttvm;
            while (temp.ParentTreeItem != null) {
                indentCount++;
                temp = temp.ParentTreeItem;
            }
            int indentSize = 30;
            return new Thickness((double)(indentCount*indentSize),0,0,0);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            return null;
        }
    }
}
