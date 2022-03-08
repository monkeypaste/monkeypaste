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
            if (item == null || container == null) {
                return null;
            }

            var aivm = item as MpActionViewModelBase;
            if (aivm == null) {
                return null;
            }

            string resourceKeyStr = "PropertiesTemplate";

            switch (aivm.ActionType) {
                case MpActionType.Trigger:
                    switch((MpTriggerType)aivm.ActionObjId) {
                        case MpTriggerType.ContentAdded:
                            resourceKeyStr = "ContentAdd" + resourceKeyStr;
                            break;
                        case MpTriggerType.ContentTagged:
                            resourceKeyStr = "ContentTagged" + resourceKeyStr;
                            break;
                        case MpTriggerType.FileSystemChange:
                            resourceKeyStr = "FileSystem" + resourceKeyStr;
                            break;
                        default:
                            throw new Exception("Uknown Trigger type: " + aivm.ActionObjId);
                    }
                    break;
                case MpActionType.Compare:
                    resourceKeyStr = "Compare" + resourceKeyStr;
                    break;
                case MpActionType.Analyze:
                    resourceKeyStr = "Analyze" + resourceKeyStr;
                    break;
                case MpActionType.Classify:
                    resourceKeyStr = "Classify" + resourceKeyStr;
                    break;
                case MpActionType.FileWriter:
                    resourceKeyStr = "FileWriter" + resourceKeyStr;
                    break;
            }

            var b = container.GetVisualAncestor<Border>();
            if(b == null || !b.Resources.Contains(resourceKeyStr)) {
                return null;
            }

            return b.Resources[resourceKeyStr] as DataTemplate;
        }
    }
}
