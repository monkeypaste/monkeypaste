using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using MonkeyPaste;

namespace MpWpfApp {
    public class MpContextMenuItemSelector : DataTemplateSelector {
        public DataTemplate MenuItemTemplate { get; set; }
        public DataTemplate SeparatorTempalte { get; set; }


        public override DataTemplate SelectTemplate(object item, DependencyObject container) {
            if(item == null || container == null) {
                return null;
            }

            var cmivm = item as MpContextMenuItemViewModel;

            return cmivm.IsSeparator ? SeparatorTempalte : MenuItemTemplate;
        }
    }
}
