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

        void CopyItemViewModels_SelectionChanged(System.Object sender, Xamarin.Forms.SelectionChangedEventArgs e)
        {            
            var viewModel = (MpCopyItemCollectionViewModel)BindingContext;
            if(e.CurrentSelection.Count == 0) {
                viewModel.SelectedCopyItemViewModel = null;
            } else {
                viewModel.SelectedCopyItemViewModel = e.CurrentSelection[0] as MpCopyItemViewModel;
            }

            MpConsole.Instance.WriteLine(@"CopyItem: " + viewModel.SelectedCopyItemViewModel.CopyItem.ItemText + " selected");
            //viewModel.AddFavorites.Execute(Photos.SelectedItems.Select(x => (Photo)x).ToList());
            //DisplayAlert("Added", "Selected photos has been added to favorites", "OK");
        }
    }
}