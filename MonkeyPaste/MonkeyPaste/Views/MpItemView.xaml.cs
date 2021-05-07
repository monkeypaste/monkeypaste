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
    public partial class MpItemView : ContentPage {
        public MpItemView(MpItemViewModel viewModel) {
            InitializeComponent();
            viewModel.Navigation = Navigation;
            BindingContext = viewModel;
            
        }
    }
}