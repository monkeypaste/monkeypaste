using System.Windows;
using System.Windows.Controls;
using MonkeyPaste;

namespace MpWpfApp {
    public class MpSelectorItemDataTemplateSelector : DataTemplateSelector {
        public override DataTemplate SelectTemplate(object item, DependencyObject container) {
            var cmivm = item as MpMenuItemViewModel;
            if (cmivm == null) {
                return null;
            }
            string keyStr = string.Empty;

            if (cmivm.IsHeaderedSeparator) {
                keyStr = "HeaderedSeperatorSelectorItem";
            } else if (cmivm.IsSeparator) {
                keyStr = "SeperatorSelectorItem";
            } else if (!string.IsNullOrEmpty(cmivm.IconResourceKey)) {
                keyStr = "SelectorItem";
            }  else if (cmivm.IconId > 0) {
                keyStr = "UserSelectorItem";
            } else {
                keyStr = "SelectorItem";
            }
            var result = (DataTemplate)Application.Current.Resources[keyStr];
            return result;
        }
    }
}
