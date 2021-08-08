using Rg.Plugins.Popup.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace MonkeyPaste {
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MpContextMenuView : Rg.Plugins.Popup.Pages.PopupPage {
        #region Private Variables
        private DateTime _lastItemTapTime;
        #endregion

        #region Properties
        public bool IsMenuVisible { get; set; } = false;
        #endregion

        #region Events
        public event EventHandler OnGlobalTouch;
        #endregion

        public MpContextMenuView() {
            InitializeComponent();
            _lastItemTapTime = DateTime.Now;
        }

        protected override void OnAppearing() {
            base.OnAppearing();
            IsMenuVisible = true;

            OnGlobalTouch += MpContextMenuViewModel_OnGlobalTouch;
            (Application.Current.MainPage as MpMainShell).GlobalTouchService.Subscribe(OnGlobalTouch);
        }

        protected override void OnDisappearing() {
            base.OnDisappearing();
            IsMenuVisible = false;

            OnGlobalTouch -= MpContextMenuViewModel_OnGlobalTouch;
            (Application.Current.MainPage as MpMainShell).GlobalTouchService.Unsubscribe(OnGlobalTouch);
        }

        private void MpContextMenuViewModel_OnGlobalTouch(object sender, EventArgs e) {
            if(!IsMenuVisible) {
                return;
            } 
            var globalTouchTime = DateTime.Now;
            var timer = new System.Timers.Timer();
            timer.Interval = 100;
            timer.AutoReset = false;
            timer.Elapsed += (s, e) => {
                if(_lastItemTapTime < globalTouchTime && IsMenuVisible) {
                    Task.Run(async () => {
                        await PopupNavigation.Instance.PopAllAsync();
                    });
                }
            };
            timer.Start();
            //var cmvm = BindingContext as MpContextMenuViewModel;
            var globalTouchPoint = (e as MpTouchEventArgs<Point>).EventData;
            //var locationFetcher = DependencyService.Get<MpIUiLocationFetcher>();
            //var menuOrigin = locationFetcher.GetCoordinates(this as VisualElement);
            //var globalRect = new Rect(menuOrigin, new Size(cmvm.Width,cmvm.Height));
            //MpConsole.WriteLine($"Global Touch: {globalTouchPoint.X}:{globalTouchPoint.Y}");
            //MpConsole.WriteLine(@"Menu Rect: "+globalRect.ToString());
            //if (!globalRect.Contains(globalTouchPoint)) {
            //    Task.Run(async () => {
            //        await PopupNavigation.Instance.PopAllAsync();
            //    });
            //}
        }

        protected override bool OnBackgroundClicked() {
            //return base.OnBackgroundClicked();
            //Task.Run(async () => {
            //    await PopupNavigation.Instance.PopAllAsync();
            //});
            return true;
        }

        private void TapGestureRecognizer_Tapped(object sender, EventArgs e) {
            _lastItemTapTime = DateTime.Now;
        }
    }
}