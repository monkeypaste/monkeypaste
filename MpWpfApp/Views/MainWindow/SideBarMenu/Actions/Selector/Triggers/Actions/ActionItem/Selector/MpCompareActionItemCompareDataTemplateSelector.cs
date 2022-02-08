using System.Windows;
using System.Windows.Controls;
using MonkeyPaste;

namespace MpWpfApp {
    public class MpCompareActionItemCompareDataTemplateSelector : DataTemplateSelector {

        public override DataTemplate SelectTemplate(object item, DependencyObject container) {
            if (item == null) {
                return null;
            }

            var cavm = item as MpCompareActionViewModelBase;
            if (cavm == null) {
                return null;
            }

            string keyStr = "TextBoxCompareDataTemplate";
            switch (cavm.ComparePropertyPathType) {
                case MpComparePropertyPathType.ItemType:
                    keyStr = "ItemTypeCompareDataTemplate";
                    break;
            }

            var result = (container as FrameworkElement).GetVisualAncestor<Grid>().Resources[keyStr] as DataTemplate;
            return result;
        }
    }
}
