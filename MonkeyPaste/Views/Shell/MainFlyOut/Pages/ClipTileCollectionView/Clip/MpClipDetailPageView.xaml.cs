using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace MonkeyPaste {
    [XamlCompilation(XamlCompilationOptions.Compile)]
    [QueryProperty(nameof(CopyItemId), "CopyItemId")]
    public partial class MpCopyItemDetailPageView : ContentPage {
        public int CopyItemId {
            set {
                LoadCopyItem(value);
            }
        }

        public MpCopyItemDetailPageView() : base() {
            InitializeComponent();
        }
        protected override async void OnDisappearing() {
            var cidpvm = BindingContext as MpCopyItemDetailPageViewModel;
                        
            //cidpvm.CopyItem.ItemPlainText = await cidpvm.StopMessageListener();
            cidpvm.CopyItem.ItemText = await cidpvm.EvaluateEditorJavaScript($"getText()");
            cidpvm.CopyItem.ItemText = cidpvm.CopyItem.ItemText.Replace("\"", string.Empty);

            var itemHtml = await cidpvm.EvaluateEditorJavaScript($"getHtml()");
            // Unescape that damn Unicode Java bull.
            itemHtml = Regex.Replace(
                itemHtml,
                @"\\[Uu]([0-9A-Fa-f]{4})",
                m => char.ToString((char)ushort.Parse(m.Groups[1].Value, NumberStyles.AllowHexSpecifier)));
            itemHtml = Regex.Unescape(itemHtml);
            itemHtml = itemHtml.Replace("\"", string.Empty);
            cidpvm.CopyItem.ItemHtml = itemHtml;

            ContentEditorWebView = null;
            cidpvm.Dispose();

            await MpDb.Instance.UpdateItemAsync<MpCopyItem>(cidpvm.CopyItem);

            base.OnDisappearing();
        }

        private async void LoadCopyItem(int ciid) {
            try {
                var ci = await MpCopyItem.GetCopyItemById(ciid);
                var cidpvm = new MpCopyItemDetailPageViewModel(ci);
                BindingContext = cidpvm;
                //init when evaljs is non-null
                
            } catch (Exception) {
                MpConsole.WriteLine($"Failed to load copy item {ciid}.");
            }
        }

        private void ContentEditorWebView_Navigated(object sender, WebNavigatedEventArgs e) {
            var cidpvm = BindingContext as MpCopyItemDetailPageViewModel;
            //cidpvm.StartMessageListener();
        }
    }
}