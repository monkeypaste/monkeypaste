using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace MonkeyPaste {
    [XamlCompilation(XamlCompilationOptions.Compile)]
    [QueryProperty(nameof(TagId), "TagId")]
    public partial class MpCopyItemCollectionView : ContentView {
        public int TagId {
            set {
                LoadTagItems(value);
            }
        }
        public MpCopyItemCollectionView() {
            InitializeComponent();
            BindingContext = new MpCopyItemCollectionViewModel(MpDb.Instance);
        }

        void LoadTagItems(int tagId) {
            (BindingContext as MpCopyItemCollectionViewModel).TagId = tagId;
        }

        void CopyItemViewModels_SelectionChanged(System.Object sender, Xamarin.Forms.SelectionChangedEventArgs e) {
            var cicvm = (MpCopyItemCollectionViewModel)BindingContext;
            if (e.CurrentSelection.Count == 0) {
                cicvm.SelectedCopyItemViewModel = null;
            } else {
                var scivm = e.CurrentSelection[0] as MpCopyItemViewModel;
                cicvm.SelectedCopyItemViewModel = scivm;
                //cicvm.ItemSelected.Execute(scivm);
            }

            MpConsole.WriteTraceLine(@"CopyItem: " + cicvm.SelectedCopyItemViewModel.CopyItem.ItemPlainText + " selected");
        }
    }
}