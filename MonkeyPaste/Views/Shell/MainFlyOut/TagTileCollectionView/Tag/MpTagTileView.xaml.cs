using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace MonkeyPaste {
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MpTagTileView : ContentView {
        public event EventHandler<object> OnSwipeStarted;

        public MpTagTileView() {
            InitializeComponent();
            BindingContextChanged += MpTagTileView_BindingContextChanged;
            (Application.Current.MainPage as MpMainShell).OnShellDisappearing += MpTagTileView_OnShellDisappearing;
        }


        private void MpTagTileView_OnShellDisappearing(object sender, object e) {
            TagTileSwipeView.Close();
        }

        private void MpTagTileView_BindingContextChanged(object sender, EventArgs e) {
            if(BindingContext != null) {
                (BindingContext as MpTagTileViewModel).PropertyChanged += MpTagTileView_PropertyChanged;
            }            
        }


        private void DropGestureRecognizer_Drop(object sender, DropEventArgs e) {
            e.Handled = true;
        }

        private void MpTagTileView_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            var ttvm = (sender as MpTagTileViewModel);
            switch(e.PropertyName) {
                case nameof(ttvm.IsNameReadOnly):
                    if(!ttvm.IsNameReadOnly) {
                        TagNameEntry.Focus();
                        Dispatcher.BeginInvokeOnMainThread(() => {
                            TagNameEntry.CursorPosition = 0;
                            TagNameEntry.SelectionLength = TagNameEntry.Text != null ? TagNameEntry.Text.Length : 0;
                         });
                    } 
                    break;
            }
        }

        public void CloseSwipeView() {
            TagTileSwipeView.Close();
        }

        private void SwipeView_SwipeStarted(object sender, SwipeStartedEventArgs e) {
            OnSwipeStarted?.Invoke(sender, e);
        }

        private void TagNameEntry_Completed(object sender, EventArgs e) {
            var ttvm = (BindingContext as MpTagTileViewModel);
            ttvm.IsNameReadOnly = true;
        }
    }
}