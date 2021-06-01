using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace MonkeyPaste {
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MpClipCollectionPageView : ContentPage {
        public MpClipCollectionPageView() {
            InitializeComponent();
            BindingContextChanged += (s, e) => {
                if (BindingContext == null) {
                    return;
                }
                var cicvm = BindingContext as MpClipTileCollectionPageViewModel;
                ClipViewModelSearchHandler.PropertyChanged += cicvm.OnSearchQueryChanged;
            };   
        }
    }
}