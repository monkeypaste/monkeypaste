using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using MonkeyPaste;

namespace MpWpfApp {

    public class MpActionDesignerItemTemplateSelector : DataTemplateSelector {
        
        public override DataTemplate SelectTemplate(object item, DependencyObject container) {
            if(item == null) {
                return null;
            }

            var aivm = item as MpActionViewModelBase;
            if(aivm == null) {
                return null;
            }
            string keyStr = string.Format(@"{0}DesignerTemplate", aivm.ActionType.ToString());

            var result = (container as FrameworkElement).GetVisualAncestor<UserControl>().Resources[keyStr] as DataTemplate;
            return result;
        }
    }
}
