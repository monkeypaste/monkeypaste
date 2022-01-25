using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace MpWpfApp {
    public class MpActionItemTemplateSelector : DataTemplateSelector {
        public override DataTemplate SelectTemplate(object item, DependencyObject container) {
            var mvm = item as MpActionViewModelBase;
            if (mvm == null) {
                return null;
            }
            string templateKeyName = mvm.ActionType.ToString() + "Template";
            return (container as FrameworkElement).GetVisualAncestor<UserControl>().Resources[templateKeyName] as DataTemplate;
        }
    }
}
