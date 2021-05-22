using System;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;


namespace MonkeyPaste {
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class App : Application {
        public App() {
            InitializeComponent();
            MainPage = MpResolver.Resolve<MpMainShell>();
        }

        protected override void OnStart() {
            // Register for clipboard changes, be sure to unsubscribe when needed
        }
        protected override void OnSleep() {
        }

        protected override void OnResume() {
        }
    }
}
