using Rg.Plugins.Popup.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace MonkeyPaste {
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MpCopyItemView : ContentView {
        MpContextMenuView cm;

        #region Events
        public event EventHandler OnGlobalTouch;
        #endregion

        public MpCopyItemView() : this(new MpCopyItemViewModel()) { }

        public MpCopyItemView(MpCopyItemViewModel viewModel) : base() {
            InitializeComponent();
            BindingContextChanged += MpCopyItemView_BindingContextChanged;
            BindingContext = viewModel;

            
        }

        private void MpCopyItemView_BindingContextChanged(object sender, EventArgs e) {
            if (BindingContext != null && BindingContext is MpCopyItemViewModel) {
                cm = new MpContextMenuView();
                cm.BindingContext = (BindingContext as MpCopyItemViewModel).ContextMenuViewModel;
                //OnGlobalTouch += MpCopyItemView_OnGlobalTouch;
                //(Application.Current.MainPage as MpMainShell).GlobalTouchService.Subscribe(OnGlobalTouch);
            } else {
               // OnGlobalTouch -= MpCopyItemView_OnGlobalTouch;
                //(Application.Current.MainPage as MpMainShell).GlobalTouchService.Unsubscribe(OnGlobalTouch);
            }
        }

        //private void MpCopyItemView_OnGlobalTouch(object sender, EventArgs e) {
        //    if (BindingContext == null) {
        //        return;
        //    }
        //    var gtp = (e as MpTouchEventArgs<Point>).EventData.GetScreenPoint(this);
        //    //gtp = gtp.GetScreenPoint(this);
        //    var thisRect = this.Bounds;
        //    if (thisRect.Contains(gtp)) {
        //        (BindingContext as MpCopyItemViewModel).IsSelected = true;
        //    }
        //}

        private void ContextMenuButton_Clicked(object sender, EventArgs e) {
            Task.Run(async () => {
                if (cm.IsMenuVisible) {
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
                await PopupNavigation.Instance.PushAsync(cm, false);
            });
        }
    }
}