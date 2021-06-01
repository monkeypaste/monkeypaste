using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace MonkeyPaste {
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MpTagView : ContentView {
        public MpTagView() {
            InitializeComponent();
        }

        private void SwipeView_SwipeStarted(object sender, SwipeStartedEventArgs e) {
            if (sender != null && sender is MpTagTileViewModel ttvm) {
                ttvm.IsSelected = true;
            }
        }
    }
}