using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using MonkeyPaste;

namespace MpWpfApp {
    public class MpNotificationTemplateSelector : DataTemplateSelector {
        public override DataTemplate SelectTemplate(object item, DependencyObject container) {
            if(item == null || container == null) {
                return null;
            }
            string keyString;
            
            if(item is MpUserActionNotificationViewModel) {
                keyString = @"UserActionNotificationTemplate";
            } else if(item is MpLoaderNotificationViewModel) {
                keyString = @"LoaderNotificationTemplate";
            } else {
                throw new Exception("Unknown notification template");
            }

            var uc = container.GetVisualAncestor<MpUserControl>();
            if(uc == null) {
                return null;
            }
            return uc.Resources[keyString] as DataTemplate;
        }
    }
}
