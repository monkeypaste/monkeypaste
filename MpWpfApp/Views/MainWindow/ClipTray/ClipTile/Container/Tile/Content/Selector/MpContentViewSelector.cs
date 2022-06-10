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
    public class MpContentViewSelector : DataTemplateSelector {

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
            if (ctvm.IsChromiumEditor) {
                keyStr = "QuillViewTemplate";
            } else {
                keyStr = "ContentViewTemplate";
            }
            var fe = container as FrameworkElement;
            if(fe == null) {
                return null;
            }
            var pfe = fe.GetVisualAncestor<DockPanel>();
            if(pfe == null) {
                return null;
            }

            var result = pfe.Resources[keyStr] as DataTemplate;
            return result;
        }
    }
}
