using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace MonkeyPaste {
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MpMainPage : FlyoutPage {

        public static bool IsLoaded { get; set; } = false;

        public MpSettingsPageView SettingsPageView { get; set; }

        public MpIKeyboardInteractionService LayoutService { get; set; }
        public MpIGlobalTouch GlobalTouchService { get; set; }
        public static MpINativeInterfaceWrapper NativeWrapper { get; set; }
        public MpIDbInfo DbInfo { get; set; }

        public MpMainPage(MpINativeInterfaceWrapper niw) {
            IsLoaded = true;

            NativeWrapper = niw;
            GlobalTouchService = niw.GlobalTouch;
            LayoutService = niw.KeyboardInteractionService;
            DbInfo = niw.DbInfo;

            InitializeComponent();

            BindingContext = new MpMainPageViewModel();
            if (Device.RuntimePlatform == Device.UWP) {
                FlyoutLayoutBehavior = FlyoutLayoutBehavior.Popover;
            }
        }

        private void TagCollectionView_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            //if(e.CurrentSelection.Count > 0) {
            //    var item = e.CurrentSelection.First() as MpMainPageFlyoutMenuItem;
            //    if (item == null)
            //        return;

            //    var page = (Page)Activator.CreateInstance(item.TargetType);
            //    page.Title = item.Title;

            //    Detail = new NavigationPage(page);
            //    IsPresented = false;

            //    FlyoutPage.ListView.SelectedItem = null;
            //}
            
        }

        //private void ListView_ItemSelected(object sender, SelectedItemChangedEventArgs e) {
        //    var item = e.SelectedItem as MpMainPageFlyoutMenuItem;
        //    if (item == null)
        //        return;

        //    var page = (Page)Activator.CreateInstance(item.TargetType);
        //    page.Title = item.Title;

        //    Detail = new NavigationPage(page);
        //    IsPresented = false;

        //    FlyoutPage.ListView.SelectedItem = null;
        //}
    }
}