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
        MpContextMenuView cm;

        private double _minDragY = 10.0;

        #region Events
        public event EventHandler OnGlobalTouch;
        #endregion

        public MpTagTileView() {
            InitializeComponent();

            BindingContextChanged += MpTagTileView_BindingContextChanged;
        }


        private void MpTagTileView_BindingContextChanged(object sender, EventArgs e) {
            if (BindingContext != null && BindingContext is MpTagTileViewModel ttvm) {
                ttvm.PropertyChanged += MpTagTileView_PropertyChanged;

                PopupNavigation.Instance.Pushing += (sender, e) => Debug.WriteLine($"[Popup] Pushing: {e.Page.GetType().Name}");
                PopupNavigation.Instance.Pushed += (sender, e) => Debug.WriteLine($"[Popup] Pushed: {e.Page.GetType().Name}");

                cm = new MpContextMenuView();
                cm.BindingContext = (BindingContext as MpTagTileViewModel).ContextMenuViewModel;

                OnGlobalTouch += MpTagTileView_OnGlobalTouch;
                (Application.Current.MainPage as MpMainShell).GlobalTouchService.Subscribe(OnGlobalTouch);
            }
        }

        private void MpTagTileView_OnGlobalTouch(object sender, EventArgs e) {
            //var locationFetcher = DependencyService.Get<MpIUiLocationFetcher>();
            //var dpi = locationFetcher.GetDensity(this);
            //var gtp = (e as MpTouchEventArgs<Point>).EventData;
            //gtp = new Point(gtp.X / dpi, gtp.Y / dpi);
            //var thisOrigin = locationFetcher.GetCoordinates(this);
            //var thisRect = new Rectangle(thisOrigin, new Size(this.Width, this.Height));

            //if (thisRect.Contains(gtp)) {
            //    (BindingContext as MpTagTileViewModel).IsSelected = true;
            //}
            var gtp = (e as MpTouchEventArgs<Point>).EventData.GetScreenPoint(this);
            var thisRect = this.GetScreenRect();
            if (thisRect.Contains(gtp)) {
                (BindingContext as MpTagTileViewModel).IsSelected = true;
            }
        }

        private void DropGestureRecognizer_Drop(object sender, DropEventArgs e) {
            e.Handled = true;
            TagTileDragGestureRecognizer.CanDrag = false;
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


        private void TagNameEntry_Completed(object sender, EventArgs e) {
            var ttvm = (BindingContext as MpTagTileViewModel);
            ttvm.IsNameReadOnly = true;
        }

        private async void ContextMenuButton_Clicked(object sender, EventArgs e) {
            if(cm.IsMenuVisible) {
                return;
            }
            var locationFetcher = DependencyService.Get<MpIUiLocationFetcher>();
            var location = locationFetcher.GetCoordinates(sender as VisualElement);
            var cmvm = cm.BindingContext as MpContextMenuViewModel;
            //cmvm.Width = 300;
            //cmvm.Height = 300;
            var w = cmvm.Width;
            var h = cmvm.Height;
            var bw = ContextMenuButton.Width;
            cm.AnchorX = 0;// location.X - w;
            cm.AnchorY = 0;
            cm.TranslationX = location.X - w + bw - cmvm.Padding.Left;
            cm.TranslationY = location.Y - cmvm.ItemHeight + cmvm.Padding.Top + cmvm.Padding.Bottom;
            cm.TranslationX = Math.Max(0, cm.TranslationX);
            await PopupNavigation.Instance.PushAsync(cm);
        }

        private void PanGestureRecognizer_PanUpdated(object sender, PanUpdatedEventArgs e) {
            switch (e.StatusType) {
                case GestureStatus.Running:
                    if(Math.Abs(e.TotalY) > _minDragY) {
                        TagTileDragGestureRecognizer.CanDrag = true;
                    }
                    break;
            }
        }
    }
}