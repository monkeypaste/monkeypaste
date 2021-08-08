using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace MonkeyPaste {
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MpCopyItemCollectionPageView : ContentPage {

        MpCopyItemTileCollectionPageViewModel cicpvm {
            get {
                if(BindingContext == null) {
                    return null;
                }
                return BindingContext as MpCopyItemTileCollectionPageViewModel;
            }
        }
        public MpCopyItemCollectionPageView() {
            InitializeComponent();

            BindingContextChanged += MpCopyItemCollectionPageView_BindingContextChanged;
        }

        private void Clipboard_ClipboardContentChanged(object sender, EventArgs e) {
            MpPopupMessagePage.ShowPopupMessage("Copied to Clipboard");

            cicpvm.OnPropertyChanged(nameof(cicpvm.ClipboardToolbarIcon));
        }

        private void MpCopyItemCollectionPageView_BindingContextChanged(object sender, EventArgs e) {
            if (BindingContext == null) {
                return;
            }

            CopyItemViewModelSearchHandler.PropertyChanged += cicpvm.OnSearchQueryChanged;
            CopyItemViewModelSearchHandler.Focused += CopyItemViewModelSearchHandler_Focused;
            Clipboard.ClipboardContentChanged += Clipboard_ClipboardContentChanged;
        }

        private void CopyItemViewModelSearchHandler_Focused(object sender, EventArgs e) {
            cicpvm.OnPropertyChanged(nameof(cicpvm.ClipboardToolbarIcon));
        }
        protected override void OnAppearing() {
            base.OnAppearing();

            if(cicpvm != null) {
                var scivm = cicpvm.SelectedCopyItemViewModel;
                //occurs when navigating back from editing a copy item
                cicpvm.SetTag(cicpvm.TagId);
                if(scivm != null) {
                    scivm.IsSelected = true;
                }
            }
        }
    }
}