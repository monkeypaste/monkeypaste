using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace MonkeyPaste {
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MpTagCollectionView : ContentView {
        public MpTagCollectionView() {
            InitializeComponent();
            //BindingContext = ((App.Current.MainPage as MpMainShell).BindingContext as MpMainShellViewModel).TagCollectionViewModel;
        }

        private async void MenuItemsListView_ItemSelected(object sender, SelectedItemChangedEventArgs e) {
            var stvm = e.SelectedItem as MpTagItemViewModel;
            //MpResolver.Resolve<MpCopyItemCollectionViewModel>().TagId = stvm.Tag.Id;
            //await Shell.Current.GoToAsync($"//tagitems?TagId={stvm.Tag.Id}");
        }
    }
}