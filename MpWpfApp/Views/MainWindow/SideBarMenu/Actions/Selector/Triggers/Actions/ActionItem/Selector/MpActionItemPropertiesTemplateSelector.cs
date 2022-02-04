using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using MonkeyPaste;

namespace MpWpfApp {
    public class MpActionItemPropertiesTemplateSelector : DataTemplateSelector {
        public override DataTemplate SelectTemplate(object item, DependencyObject container) {
            if (item == null) {
                return null;
            }
            var fe = container as FrameworkElement;

            var aivm = item as MpActionViewModelBase;
            if (aivm == null) {
                return null;
            }
            string keyStr = string.Format(@"{0}PropertiesTemplate", aivm.ActionType.ToString());

            if (fe.Name == "ActionPropertiesContainer" && keyStr == "None") {
                return null;
            }
            var result = (container as FrameworkElement).GetVisualAncestor<Border>().Resources[keyStr] as DataTemplate;
            return result;
        }
    }
}
