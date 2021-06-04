using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace MonkeyPaste {
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MpClipView : ContentView
    {
        public MpClipView() : this(new MpClipTileViewModel()) { }

        public MpClipView(MpClipTileViewModel viewModel) : base()
        {
            InitializeComponent();
            BindingContext = viewModel;
        }

        private void SwipeView_SwipeStarted(object sender, SwipeStartedEventArgs e) {
            if (sender != null && sender is SwipeView sv) {
                var ctvm = sv.BindingContext as MpClipTileViewModel;
                ctvm.IsSelected = true;
            }
        }
    }
}