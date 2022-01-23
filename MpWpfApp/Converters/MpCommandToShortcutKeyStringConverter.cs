using System;
using System.Globalization;
using System.Windows.Data;
using System.Linq;
using System.Windows.Input;

namespace MpWpfApp {
    public class MpCommandToShortcutKeyStringConverter : IValueConverter {
        //returns primary source by default but secondary w/ parameter of 'SecondarySource' 
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            if (value is ICommand icommand) {
                var svm = MpShortcutCollectionViewModel.Instance.Shortcuts.FirstOrDefault(x => x.Command == icommand);
                return svm == null ? null : svm.KeyString;
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            return null;
        }
    }
}
