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
            if(cidpvm == null || cidpvm.UpdateTimer == null) {
                //for some reason when going to edit a new item it automatically
                //disappears maybe because its receiving more taps than required?
                //so this should catch it
                base.OnDisappearing();
                return;
            }
            cidpvm.UpdateTimer.Stop();

            cidpvm.CopyItem.ItemText = cidpvm.Text; 
            cidpvm.CopyItem.ItemText = cidpvm.CopyItem.ItemText.Replace("\"", string.Empty);

            var templatesStr = cidpvm.Templates.Replace("\"", string.Empty);
            templatesStr = templatesStr.Substring(1, templatesStr.Length - 2);
            var templates = templatesStr.Split(new string[] { "}" }, StringSplitOptions.RemoveEmptyEntries);
            var citl = new List<MpCopyItemTemplate>();
            foreach(var template in templates) {
                var templateParts = template.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);
                var templateColor = Color.FromHex(templateParts[0].Substring(templateParts[0].IndexOf(':') + 1).Replace("#",string.Empty));
                var templateId = Convert.ToInt32(templateParts[1].Substring(templateParts[1].IndexOf(':') + 1));
                var templateName = templateParts[2].Substring(templateParts[2].IndexOf(':') + 1);
                var templateText = templateParts[3].Substring(templateParts[3].IndexOf(':') + 1);

                var t = new MpCopyItemTemplate() {
                    Id = templateId > 0 ? templateId : 0,
                    CopyItemId = cidpvm.CopyItem.Id,
                    Color = new MpColor(templateColor),
                    TemplateName = templateName,
                    TemplateText = templateText
                };
                await MpDb.Instance.AddOrUpdateAsync<MpCopyItemTemplate>(t);
                citl.Add(t);
            }
            var itemHtml = cidpvm.Html;//await cidpvm.EvaluateEditorJavaScript($"getHtml()");
            // Unescape that damn Unicode Java bull.
            itemHtml = Regex.Replace(
                itemHtml,
                @"\\[Uu]([0-9A-Fa-f]{4})",
                m => char.ToString((char)ushort.Parse(m.Groups[1].Value, NumberStyles.AllowHexSpecifier)));
            itemHtml = Regex.Unescape(itemHtml);
            itemHtml = itemHtml.Replace("\"", char.ToString('"'));
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