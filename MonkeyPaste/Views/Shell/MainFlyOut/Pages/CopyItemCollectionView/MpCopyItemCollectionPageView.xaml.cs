using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace MonkeyPaste {
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MpCopyItemCollectionPageView : ContentPage {
        public MpCopyItemCollectionPageView() {
            InitializeComponent();
            BindingContextChanged += (s, e) => {
                if (BindingContext == null) {
                    return;
                }
                var cicvm = BindingContext as MpCopyItemCollectionViewModel;
                CopyItemViewModelSearchHandler.PropertyChanged += cicvm.OnSearchQueryChanged;
            };   
        }
    }
}