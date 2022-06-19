using System.Windows;
using System.Windows.Controls;
using MonkeyPaste.Common.Wpf;
using MonkeyPaste;

namespace MpWpfApp {
    public class MpPasteTemplateDetailViewSelector : DataTemplateSelector {

        public override DataTemplate SelectTemplate(object item, DependencyObject container) {
            if (item == null || container == null) {
                return null;
            }

            var tvm = item as MpTextTemplateViewModelBase;
            if (tvm == null) {
                return null;
            }

            string keyStr = tvm.TextTemplateType.ToString() + "Template";
            var fe = container as FrameworkElement;
            if (fe == null) {
                return null;
            }
            var pfe = fe.GetVisualAncestor<Grid>();
            if (pfe == null) {
                return null;
            }

            var result = pfe.Resources[keyStr] as DataTemplate;
            return result;
        }
    }
}
