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
    [QueryProperty(nameof(CopyItemId), nameof(CopyItemId))]
    [QueryProperty(nameof(IsFillingOutTemplates), nameof(IsFillingOutTemplates))]
    public partial class MpCopyItemDetailPageView : ContentPage {
        private bool _fillTemplates = false;

        public int CopyItemId {
            set {
                LoadCopyItem(value);
            }
        }

        public int IsFillingOutTemplates {
            set {
                _fillTemplates = value == 1;
            }
        }

        public MpCopyItemDetailPageView() : this(false) {  }

        public MpCopyItemDetailPageView(bool fillTemplates) : base() {
            _fillTemplates = fillTemplates;
            InitializeComponent();
        }
        protected override async void OnDisappearing() {
            var cidpvm = BindingContext as MpCopyItemDetailPageViewModel;
            if(cidpvm == null || cidpvm.UpdateTimer == null || string.IsNullOrEmpty(cidpvm.Html)) {
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
                    CopyItem = cidpvm.CopyItem,
                    HexColor = templateColor.ToHex(),
                    TemplateName = templateName,
                    TemplateText = templateText
                };
                await MpDb.Instance.AddOrUpdateAsync<MpCopyItemTemplate>(t);
                citl.Add(t);
            }
            var itemHtml = cidpvm.Html;
            // Unescape that damn Unicode Java bull.
            itemHtml = Regex.Replace(
                itemHtml,
                @"\\[Uu]([0-9A-Fa-f]{4})",
                m => char.ToString((char)ushort.Parse(m.Groups[1].Value, NumberStyles.AllowHexSpecifier)));
            itemHtml = Regex.Unescape(itemHtml);
            itemHtml = Regex.Unescape(itemHtml);
            itemHtml = itemHtml.Replace('\"', '"');
            itemHtml = itemHtml.Replace('"', '\'');
            //itemHtml = itemHtml.Replace("\"", char.ToString('"'));

            //var sb = new StringBuilder();
            //for(int i = 0;i < itemHtml.Length-1;i++) {
            //    if(itemHtml[i] == '\"') {
            //        continue;
            //    } else {
            //        sb.Append(itemHtml[i]);
            //    }
                
            //}
            //itemHtml = sb.ToString();
            for (int i = 1; i <= citl.Count; i++) {
                int tId = -i;
                string oldVal = "templateid='" + tId + "'";
                string newVal = "templateid='" + citl[i - 1].Id + "'";
                itemHtml = itemHtml.Replace(oldVal, newVal);
            }
            if(itemHtml.Length > 2) {
                itemHtml = itemHtml.Substring(1, itemHtml.Length - 2);
            }
            cidpvm.CopyItem.ItemHtml = itemHtml;
            cidpvm.CopyItem.Templates = citl;


            ContentEditorWebView = null;

            await MpDb.Instance.UpdateItemAsync<MpCopyItem>(cidpvm.CopyItem);

            cidpvm.Dispose();
            base.OnDisappearing();
        }

        private async void LoadCopyItem(int ciid) {
            try {
                var ci = await MpCopyItem.GetCopyItemById(ciid);
                BindingContext = new MpCopyItemDetailPageViewModel(ci);

            } catch (Exception) {
                MpConsole.WriteLine($"Failed to load copy item {ciid}.");
            }
        }

        private void ContentEditorWebView_Navigated(object sender, WebNavigatedEventArgs e) {
            if(e.Result == WebNavigationResult.Success) {
                (BindingContext as MpCopyItemDetailPageViewModel).InitEditor(_fillTemplates);
            }
            var cidpvm = BindingContext as MpCopyItemDetailPageViewModel;
            //cidpvm.StartMessageListener();
        }
    }
}