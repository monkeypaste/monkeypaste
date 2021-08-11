using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace MonkeyPaste {
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MpDefaultContextMenuItemView : ContentView {
        public MpDefaultContextMenuItemView() {
            InitializeComponent();
        }

        private void TapGestureRecognizer_Tapped(object sender, EventArgs e) {

        }
    }
}