using System.Windows;
using System.Windows.Controls;

namespace MpWpfApp {
    public class MpTextViewTemplateSelector : DataTemplateSelector {

        public override DataTemplate SelectTemplate(object item, DependencyObject container) {
            if (item == null || container == null) {
                return null;
            }

            if (item == null) {
                return null;
            }
                        
            var civm = item as MpContentItemViewModel;
            string keyStr;
            if(civm.TemplateCollection == null || civm.TemplateCollection.Templates.Count == 0) {
                keyStr = "RtbTemplate";
            } else {
                keyStr = "FlowDocumentScrollViewerTemplate";
            }
            if (civm == null) {
                return null;
            }

            var result = (container as FrameworkElement).GetVisualAncestor<Border>().Resources[keyStr] as DataTemplate;
            return result;
        }
    }
}
