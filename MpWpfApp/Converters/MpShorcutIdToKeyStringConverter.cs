using System;
using System.Globalization;
using System.Windows.Data;
using System.Linq;

namespace MpWpfApp {
    public class MpShorcutIdToKeyStringConverter : IValueConverter {
        //returns primary source by default but secondary w/ parameter of 'SecondarySource' 
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            if(value is int shortcutId) {
                var svm = MpShortcutCollectionViewModel.Instance.Items.FirstOrDefault(x => x.ShortcutId == shortcutId);
                return svm == null ? null : svm.KeyString;
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            return null;
        }
    }
}
