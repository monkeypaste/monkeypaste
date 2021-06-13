//using MonkeyPaste.Messages;
using System.Reflection;
using Xamarin.Forms;
namespace MonkeyPaste {
    public class MoClipTileSelector : DataTemplateSelector {
        protected override DataTemplate OnSelectTemplate(object item, BindableObject container) {
            //var list = (CollectionView)container;
            //if (item is MpLocalSimpleTextMessage) {
            //    return (DataTemplate)list.Resources["LocalSimpleText"];
            //} else if (item is MpSimpleTextMessage) {
            //    return (DataTemplate)list.Resources["SimpleText"];
            //} else if (item is MpUserConnectedMessage) {
            //    return (DataTemplate)list.Resources["UserConnected"];
            //} else if (item is MpPhotoUrlMessage) {
            //    return (DataTemplate)list.Resources["Photo"];
            //} else if (item is MpPhotoMessage) {
            //    return (DataTemplate)list.Resources["LocalPhoto"];
            //}
            return null;
        }
    }
}

