using MonkeyPaste.Views;
using System;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace MonkeyPaste {
    public partial class App : Application {
        public App() {
            InitializeComponent();
            MainPage = new NavigationPage(MpResolver.Resolve<MpMainView>());
        }

        protected override void OnStart() {
            // Register for clipboard changes, be sure to unsubscribe when needed
            Clipboard.ClipboardContentChanged += OnClipboardContentChanged;

        }
        async void OnClipboardContentChanged(object sender, EventArgs e) {
            var clipText = await Clipboard.GetTextAsync();
            Console.WriteLine($"Last clipboard change at {DateTime.UtcNow:T}");
            Console.WriteLine($"With clipboard contents: {clipText}");
        }
        protected override void OnSleep() {
        }

        protected override void OnResume() {
        }
    }
}
