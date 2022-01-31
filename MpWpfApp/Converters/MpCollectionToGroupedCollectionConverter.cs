using System;
using System.Collections;
using System.Globalization;
using System.Windows.Data;
using MonkeyPaste;

namespace MpWpfApp {
    public class MpCollectionToGroupedCollectionConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            if(value == null) {
                return null;
            }
            if(parameter == null) {
                return value;
            }

            string memberPath = parameter as string;
            
            ListCollectionView lcv = new ListCollectionView(value as IList);
            lcv.GroupDescriptions.Add(new PropertyGroupDescription(memberPath));
            return lcv;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            return null;
        }
    }
}
