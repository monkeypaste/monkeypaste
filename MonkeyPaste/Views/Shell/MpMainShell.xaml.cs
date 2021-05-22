using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xamarin.Forms;
using System.Linq;
using System.Threading;

namespace MonkeyPaste
{
    public partial class MpMainShell : Shell
    {
        public MpMainShell()
        {
            InitializeComponent();
            Routing.RegisterRoute("//tagitems", typeof(MpCopyItemCollectionView));
            BindingContext = MpResolver.Resolve<MpMainShellViewModel>();
        }

        private async void MenuItemsListView_ItemSelected(object sender, SelectedItemChangedEventArgs e) {
            var stvm = e.SelectedItem as MpTagViewModel;
            MpResolver.Resolve<MpCopyItemCollectionViewModel>().TagId = stvm.Tag.Id;
            //await Shell.Current.GoToAsync($"//tagitems?TagId={stvm.Tag.Id}");
        }
    }
}
