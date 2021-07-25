using Rg.Plugins.Popup.Extensions;
using Rg.Plugins.Popup.Services;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace MonkeyPaste {
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MpTagTileView : ContentView {
        public event EventHandler<object> OnSwipeStarted;
        MpContextMenuView cm;

        public MpTagTileView() {
            InitializeComponent();

            PopupNavigation.Instance.Pushing += (sender, e) => Debug.WriteLine($"[Popup] Pushing: {e.Page.GetType().Name}");
            PopupNavigation.Instance.Pushed += (sender, e) => Debug.WriteLine($"[Popup] Pushed: {e.Page.GetType().Name}");

            BindingContextChanged += MpTagTileView_BindingContextChanged;
            (Application.Current.MainPage as MpMainShell).OnShellDisappearing += MpTagTileView_OnShellDisappearing;

            var cmvm = new MpContextMenuViewModel();
            cmvm.Items.Add(new MpContextMenuItemViewModel() { Title = "Test1", IconImageSource = ImageSource.FromResource("MonkeyPaste.Resources.Icons.monkey.png") });
            cmvm.Items.Add(new MpContextMenuItemViewModel() { Title = "Test2" });
            cmvm.Items.Add(new MpContextMenuItemViewModel() { Title = "Test3" });

            cm = new MpContextMenuView();
            cm.BindingContext = cmvm;
        }


        private void MpTagTileView_OnShellDisappearing(object sender, object e) {
            //TagTileSwipeView.Close();
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
            //TagTileSwipeView.Close();
        }

        private void SwipeView_SwipeStarted(object sender, SwipeStartedEventArgs e) {
            OnSwipeStarted?.Invoke(sender, e);
        }

        private void TagNameEntry_Completed(object sender, EventArgs e) {
            var ttvm = (BindingContext as MpTagTileViewModel);
            ttvm.IsNameReadOnly = true;
        }

        private async void ContextMenuButton_Clicked(object sender, EventArgs e) {
            var locationFetcher = DependencyService.Get<ILocationFetcher>();
            var location = locationFetcher.GetCoordinates(sender as VisualElement);
            var cmvm = cm.BindingContext as MpContextMenuViewModel;
            var w = cmvm.Width;
            var h = cmvm.Height;
            var bw = ContextMenuButton.Width;
            cm.AnchorX = 0;// location.X - w;
            cm.AnchorY = 0;
            cm.TranslationX = location.X - w + bw - cmvm.Padding.Left;
            cm.TranslationY = location.Y - cmvm.ItemHeight + cmvm.Padding.Top + cmvm.Padding.Bottom;

            await PopupNavigation.Instance.PushAsync(cm);
        }
    }
}