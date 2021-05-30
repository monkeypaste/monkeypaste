using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using System.Linq;

namespace MonkeyPaste {
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MpTagCollectionView : ContentView {
        //public MpTagCollectionViewModel TagItemCollectionViewModel => (MpTagCollectionViewModel)BindingContext;

        public MpTagCollectionView() {
            InitializeComponent();
        }

        //private void TagCollectionView_SelectionChanged(object sender, SelectionChangedEventArgs e) {
        //    var tcvm = BindingContext as MpTagCollectionViewModel;
        //    tcvm.ClearSelection();
        //    if (e.CurrentSelection != null) {
        //        var stivm = tcvm.TagViewModels.Where(x => x == e.SelectedItem).FirstOrDefault();
        //        if (stivm != null) {
        //            stivm.IsSelected = true;
        //            tcvm.SelectedTagViewModel = stivm;
        //        }
        //    }
        //}
    }
}