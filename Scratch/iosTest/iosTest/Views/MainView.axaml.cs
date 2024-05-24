using Avalonia.Controls;
using AvaloniaWebView;
using PropertyChanged;
using System;
using System.IO;
using System.Reflection;
namespace iosTest.Views {
    [DoNotNotify]
    public partial class MainView : UserControl {
        public MainView() {
            InitializeComponent();
            var localDir = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            string path = Path.Combine(localDir, "Resources", "Editor", "test.html");
            var test = File.Exists(path);
            //var uri = new System.Uri(path + "?auto_test&content-type=text", System.UriKind.Absolute);
            var uri = new System.Uri(path, System.UriKind.Absolute);
            PART_WebView.Url = uri;

            //PART_WebView.Url = new Uri("https://www.google.com", UriKind.Absolute);
            //PART_WebView.PointerReleased += PART_WebView_PointerReleased;
            //PART_WebView.NavigationCompleted += PART_WebView_NavigationCompleted;
        }

        private void PART_WebView_NavigationCompleted(object sender, WebViewCore.Events.WebViewUrlLoadedEventArg e) {
            Console.WriteLine("Webview loaded");
            Console.WriteLine(e.ToString());
        }

        private void PART_WebView_PointerReleased(object sender, Avalonia.Input.PointerReleasedEventArgs e) {

        }
    }
}