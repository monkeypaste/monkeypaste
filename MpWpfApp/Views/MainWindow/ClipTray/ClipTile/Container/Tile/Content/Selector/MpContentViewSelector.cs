using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using MonkeyPaste;

namespace MpWpfApp {
    public class MpContentViewSelector : DataTemplateSelector {

        public override DataTemplate SelectTemplate(object item, DependencyObject container) {
            if(item == null || container == null) {
                return null;
            }

            if (item == null) {
                return null;
            }

            var civm = item as MpContentItemViewModel;
            if (civm == null) {
                return null;
            }

            string keyStr = "ContentTemplate";
            switch(civm.CopyItemType) {
                case MpCopyItemType.Text:
                    //if (civm.TemplateCollection == null || civm.TemplateCollection.Templates.Count == 0) {
                    //    keyStr = "Rtb" + keyStr;
                    //} else {
                    //    keyStr = "FlowDocumentScrollViewer" + keyStr;
                    //}
                    if(false) {//civm.CopyItemData.IsStringRichText()) {
                        keyStr = "Rtb" + keyStr;
                    } else {
                        keyStr = "Quill" + keyStr;
                    }
                    
                    break;
                default:
                    keyStr = civm.CopyItemType.EnumToName() + keyStr;
                    break;
            }
            var result = (container as FrameworkElement).GetVisualAncestor<Border>().Resources[keyStr] as DataTemplate;
            return result;
        }
    }
}
