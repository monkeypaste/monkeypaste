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

            Shell.SetSearchHandler(this, null);
            BindingContextChanged += MpCopyItemCollectionPageView_BindingContextChanged;
        }

        private void MpCopyItemCollectionPageView_BindingContextChanged(object sender, EventArgs e) {
            if (BindingContext == null) {
                return;
            }
            var cicvm = BindingContext as MpCopyItemTileCollectionPageViewModel;
            cicvm.PropertyChanged += Cicvm_PropertyChanged;
            CopyItemViewModelSearchHandler.PropertyChanged += cicvm.OnSearchQueryChanged;
        }

        private void Cicvm_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            var cicvm = sender as MpCopyItemTileCollectionPageViewModel;
            switch(e.PropertyName) {
                case nameof(cicvm.SelectedCopyItemViewModel):
                    if(cicvm.SelectedCopyItemViewModel != null) {
                        Shell.SetSearchHandler(this, null); 
                    } else if(cicvm.CopyItemViewModels.Count > 0) {
                        Shell.SetSearchHandler(this, CopyItemViewModelSearchHandler);
                    }
                    break;
                case nameof(cicvm.CopyItemViewModels):
                    if (cicvm.CopyItemViewModels.Count > 0 && cicvm.SelectedCopyItemViewModel == null) {
                        Shell.SetSearchHandler(this, CopyItemViewModelSearchHandler);
                    } else {
                        Shell.SetSearchHandler(this, null);
                    }
                    break;
            }
        }

        protected override void OnAppearing() {
            base.OnAppearing();
            var cicpvm = BindingContext as MpCopyItemTileCollectionPageViewModel;
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