using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace MonkeyPaste {
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MpCopyItemCollectionView : ContentView {
        public MpCopyItemCollectionView() {
            InitializeComponent();

            //var viewModel = new MpCopyItemCollectionViewModel(MpDb.Instance);
            //viewModel.Navigation = Navigation;
            //BindingContext = viewModel;

            //CopyItemCollectionListView.S += (s, e) => CopyItemCollectionListView.SelectedItem = null;
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

            MpConsole.Instance.WriteLine(@"CopyItem: " + cicvm.SelectedCopyItemViewModel.CopyItem.ItemText + " selected");
        }
    }
}