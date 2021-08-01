using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace MonkeyPaste {
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MpCopyItemCollectionPageView : ContentPage {
        public MpCopyItemCollectionPageView() {
            InitializeComponent();
            BindingContextChanged += (s, e) => {
                if (BindingContext == null) {
                    return;
                }
                var cicvm = BindingContext as MpCopyItemTileCollectionPageViewModel;
                CopyItemViewModelSearchHandler.PropertyChanged += cicvm.OnSearchQueryChanged;
            };   
        }
        protected override void OnAppearing() {
            base.OnAppearing();
            var cicpvm = BindingContext as MpCopyItemTileCollectionPageViewModel;
            if(cicpvm != null) {
                //occurs when navigating back from editing a copy item
                cicpvm.SetTag(cicpvm.TagId);
            }
        }
    }
}