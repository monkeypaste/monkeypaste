using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace MonkeyPaste {
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MpCopyItemView : ContentView
    {
        public MpCopyItemView() : this(new MpCopyItemTileViewModel()) { }

        public MpCopyItemView(MpCopyItemTileViewModel viewModel) : base()
        {
            InitializeComponent();
            BindingContext = viewModel;
        }

        private void SwipeView_SwipeStarted(object sender, SwipeStartedEventArgs e) {
            if (sender != null && sender is SwipeView sv) {
                var ctvm = sv.BindingContext as MpCopyItemTileViewModel;
                ctvm.IsSelected = true;
            }
        }
    }
}