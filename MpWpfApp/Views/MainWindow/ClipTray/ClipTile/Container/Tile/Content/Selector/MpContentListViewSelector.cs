using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using MonkeyPaste;

namespace MpWpfApp {
    public class MpContentListViewSelector : DataTemplateSelector {

        public override DataTemplate SelectTemplate(object item, DependencyObject container) {
            if(item == null || container == null) {
                return null;
            }

            if (item == null) {
                return null;
            }

            var ctvm = item as MpClipTileViewModel;
            if (ctvm == null) {
                return null;
            }
            string keyStr;
            if (ctvm.IsTextItem) {
                keyStr = "RtbDataTemplate";
            } else {
                keyStr = "ContentListDataTemplate";
            }

            var result = (container as FrameworkElement).GetVisualAncestor<UserControl>().Resources[keyStr] as DataTemplate;
            return result;
        }
    }
}
