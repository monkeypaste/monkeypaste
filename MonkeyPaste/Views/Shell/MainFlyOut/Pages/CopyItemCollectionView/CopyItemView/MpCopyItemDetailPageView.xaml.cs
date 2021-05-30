using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        void LoadCopyItem(int id) {
            try {
                if (Application.Current.MainPage == null) {
                    return;
                }
                var ms = (Application.Current.MainPage as MpMainShell);
                if (ms.BindingContext == null) {
                    return;
                }
                var msvm = ms.BindingContext as MpMainShellViewModel;

                BindingContext = msvm.TagCollectionViewModel.CopyItemCollectionViewModel.CopyItemViewModels
                                    .FirstOrDefault(x => x.CopyItem.Id == id);
            } catch (Exception) {
                MpConsole.WriteLine($"Failed to load copy item {id}.");
            }
        }
        public MpCopyItemDetailPageView() : this(new MpCopyItemViewModel()) { }

        public MpCopyItemDetailPageView(MpCopyItemViewModel viewModel) : base() {
            InitializeComponent();
            BindingContext = viewModel;
        }
    }
}