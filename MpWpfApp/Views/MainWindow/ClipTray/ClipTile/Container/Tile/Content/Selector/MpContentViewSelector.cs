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

            string keyStr = "ContentTemplate";
            switch(ctvm.ItemType) {
                case MpCopyItemType.Text:
                    if (!MpClipTileViewModel.USING_BROWSER) {
                        //civm.CopyItemData.IsStringRichText()) {
                        keyStr = "Rtb" + keyStr;
                    } else {
                        keyStr = "Quill" + keyStr;
                    }
                    //keyStr = "Rtb" + keyStr;
                    break;
                default:
                    keyStr = ctvm.ItemType.EnumToName() + keyStr;
                    break;
            }
            var result = (container as FrameworkElement).GetVisualAncestor<Border>().Resources[keyStr] as DataTemplate;
            return result;
        }
    }
}
