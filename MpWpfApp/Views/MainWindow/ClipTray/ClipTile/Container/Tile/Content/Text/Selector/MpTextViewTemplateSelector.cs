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
                        
            var ctvm = item as MpClipTileViewModel;
            string keyStr;
            if(ctvm.TemplateCollection == null || ctvm.TemplateCollection.Items.Count == 0) {
                keyStr = "RtbTemplate";
            } else {
                keyStr = "FlowDocumentScrollViewerTemplate";
            }
            if (ctvm == null) {
                return null;
            }

            var result = (container as FrameworkElement).GetVisualAncestor<Border>().Resources[keyStr] as DataTemplate;
            return result;
        }
    }
}
