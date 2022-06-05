using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using MonkeyPaste;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Wpf;
namespace MpWpfApp {

    public class MpActionItemPropertiesTemplateSelector : DataTemplateSelector {

        public override DataTemplate SelectTemplate(object item, DependencyObject container) {
            if (item == null || container == null) {
                return null;
            }

            var aivm = item as MpActionViewModelBase;
            if (aivm == null) {
                return null;
            }

            string resourceKeyStr = aivm.ActionType.ToString() + "PropertiesTemplate";
            if (aivm.ActionType == MpActionType.Trigger) {
                resourceKeyStr = (aivm as MpTriggerActionViewModelBase).TriggerType.ToString() + "PropertiesTemplate";
            }

            var b = container.GetVisualAncestor<Border>();
            if(b == null || !b.Resources.Contains(resourceKeyStr)) {
                return null;
            }

            return b.Resources[resourceKeyStr] as DataTemplate;
        }
    }
}
