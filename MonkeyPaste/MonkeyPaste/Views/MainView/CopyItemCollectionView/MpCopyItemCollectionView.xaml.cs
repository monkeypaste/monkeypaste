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

            var viewModel = new MpCopyItemCollectionViewModel();
            viewModel.Navigation = Navigation;
            BindingContext = viewModel;

            CopyItemCollectionListView.ItemSelected += (s, e) => CopyItemCollectionListView.SelectedItem = null;
        }
    }
}