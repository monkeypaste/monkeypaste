using System;
using System.Collections.Generic;
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
            var itemText = await cidpvm.EvaluateJavascript($"getText()");
            itemText = itemText.Replace("\"", string.Empty);
            cidpvm.CopyItem.ItemPlainText = itemText;
            await MpDb.Instance.UpdateItem<MpCopyItem>(cidpvm.CopyItem);

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
    }
}