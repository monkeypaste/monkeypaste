using MonkeyPaste.Views;
using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace MonkeyPaste {
    public partial class App : Application {
        public App() {
            InitializeComponent();
            MainPage = new NavigationPage(MpResolver.Resolve<MpMainView>());
        }

        protected override void OnStart() {
        }

        protected override void OnSleep() {
        }

        protected override void OnResume() {
        }
    }
}
