//using MonkeyPaste.Messages;
using System.Reflection;
using Xamarin.Forms;
namespace MonkeyPaste {
    public class MpContextMenuTemplateSelector : DataTemplateSelector {
        protected override DataTemplate OnSelectTemplate(object item, BindableObject container) {
            var list = (ListView)container;
            if (item is MpColorChooserContextMenuItemViewModel) {
                return (DataTemplate)list.Resources["ColorPalleteMenuItem"];
            }
            return (DataTemplate)list.Resources["DefaultMenuItem"];
        }
    }
}

