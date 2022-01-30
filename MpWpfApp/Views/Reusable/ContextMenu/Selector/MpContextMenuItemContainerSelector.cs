using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using MonkeyPaste;

namespace MpWpfApp {
    public class MpContextMenuItemContainerSelector : ItemContainerTemplateSelector {
        public override DataTemplate SelectTemplate(object item, ItemsControl parentItemsControl) {
            var cmivm = item as MpMenuItemViewModel;
            if(cmivm == null) {
                return null;
            }
            string keyStr = string.Empty;

            if (cmivm.IsHeaderedSeparator) {
                keyStr = "HeaderedSeperatorMenuItem";
            } else if (cmivm.IsSeparator) {
                keyStr = "SeperatorMenuItem";
            } else if (cmivm.IsPasteToPathRuntimeItem) {
                keyStr = "PasteToPathRuntimeMenuItem";
            } else if (cmivm.IsColorPallete) {
                keyStr = "ColorPalleteMenuItem";
            } else if (!string.IsNullOrEmpty(cmivm.IconResourceKey)) {
                keyStr = "MenuItem";
            } else if (!string.IsNullOrEmpty(cmivm.IconHexStr)) {
                keyStr = cmivm.IsSelected ? "CheckedTagMenuItem" : "UncheckedTagMenuItem";
            } else if(cmivm.IconId > 0) {
                keyStr = "UserMenuItem";
            } else {
                keyStr = "MenuItem";
            }
            var result = (DataTemplate)Application.Current.Resources[keyStr];
            return result;
        }
    }
}
