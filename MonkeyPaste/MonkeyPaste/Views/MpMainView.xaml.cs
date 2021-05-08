using MonkeyPaste.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace MonkeyPaste.Views {
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MpMainView : ContentPage {
        public MpMainView(MpMainViewModel viewModel) {
            InitializeComponent();
            viewModel.Navigation = Navigation;
            BindingContext = viewModel;

            ItemsListView.ItemSelected += (s, e) => ItemsListView.SelectedItem = null;
        }
    }
}