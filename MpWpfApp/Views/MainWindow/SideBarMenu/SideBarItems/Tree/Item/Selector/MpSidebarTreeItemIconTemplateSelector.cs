using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using MonkeyPaste;

namespace MpWpfApp {
    public class MpSidebarTreeItemIconTemplateSelector : DataTemplateSelector {
        public override DataTemplate SelectTemplate(object item, DependencyObject container) {
            if(item == null || container == null) {
                return null;
            }
            string templateKeyStr;
            if(item is MpIHierarchialViewModel hvm) {
                if(hvm.IconTextOrResourceKey.IsStringResourcePath()) {
                    templateKeyStr = "ImageIconTemplate";
                } else {
                    templateKeyStr = "TagIconTemplate";
                }
            } else {
                return null;
            }

            var b = container as Border;
            if(b == null) {
                return null;
            }

            if(b.Resources.Contains(templateKeyStr)) {
                return b.Resources[templateKeyStr] as DataTemplate;
            }

            return null;
        }
    }
}
