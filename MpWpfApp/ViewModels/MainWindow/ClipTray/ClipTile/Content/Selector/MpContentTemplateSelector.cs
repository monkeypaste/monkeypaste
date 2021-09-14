using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using MonkeyPaste;

namespace MpWpfApp {
    public class MpContentTemplateSelector : System.Windows.Controls.DataTemplateSelector {
        public override System.Windows.DataTemplate SelectTemplate(object item, DependencyObject container) {
            if(item == null || container == null) {
                return null;
            }
            var tp = (container as ContentPresenter).TemplatedParent as FrameworkElement;
            var ci = (item as MpContentContainerViewModel).HeadItem.CopyItem;
            switch (ci.ItemType) {
                case MpCopyItemType.RichText:
                    var dt = (DataTemplate)(tp.Resources["RichTextTemplate"]);
                    return dt;
            }

            throw new Exception("Uknown Item Type");
        }
    }
}
