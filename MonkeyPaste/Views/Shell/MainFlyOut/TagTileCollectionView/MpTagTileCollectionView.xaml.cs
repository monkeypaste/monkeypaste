using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using System.Linq;
using System.Collections.ObjectModel;

namespace MonkeyPaste {
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MpTagCollectionView : ContentView {
        public ObservableCollection<MpTagTileView> TagTileViews { get; set; } = new ObservableCollection<MpTagTileView>();
        public MpTagCollectionView() {
            ChildAdded += MpTagCollectionView_ChildAdded;
            InitializeComponent();
        }

        private void MpTagCollectionView_ChildAdded(object sender, ElementEventArgs e) {
            if(sender is MpTagTileView ttv) {
                TagTileViews.Add(ttv);
                ttv.OnSwipeStarted += Ttv_OnSwipeStarted;
            }
        }

        private void Ttv_OnSwipeStarted(object sender, object e) {
            foreach(var ttv in TagTileViews) {
                if(ttv != sender) {
                    ttv.CloseSwipeView();
                }
            }
        }
    }
}