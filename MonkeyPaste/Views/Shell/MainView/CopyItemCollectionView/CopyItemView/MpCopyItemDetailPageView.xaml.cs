using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace MonkeyPaste {
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MpCopyItemDetailPageView : ContentPage {
        public MpCopyItemDetailPageView() : this(new MpCopyItemViewModel()) { }

        public MpCopyItemDetailPageView(MpCopyItemViewModel viewModel) : base() {
            InitializeComponent();
            BindingContext = viewModel;
        }
    }
}