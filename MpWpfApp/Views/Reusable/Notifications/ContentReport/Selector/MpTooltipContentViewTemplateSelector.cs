using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Wpf;
namespace MpWpfApp {
    public class MpTooltipContentViewTemplateSelector : DataTemplateSelector {
        public override DataTemplate SelectTemplate(object item, DependencyObject container) {
            if (item == null) {
                return null;
            }
            string resourceKey = string.Empty;
            if (item is string) {
                resourceKey = "TextBlockTooltipTemplate";
            }

            var b = container.GetVisualAncestor<Border>();
            if (b == null || !b.Resources.Contains(resourceKey)) {
                return null;
            }
            return b.Resources[resourceKey] as DataTemplate;
        }
    }
}
