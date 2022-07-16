using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using PropertyChanged;
using System.Diagnostics;
using System;
using System.IO;
using WebViewControl;

namespace MonkeyPaste.Avalonia {
    [DoNotNotify]
    public partial class App : Application {
        public App() {
            DataContext = MpAvAppViewModel.Instance;
        }
        public override void Initialize() {
            AvaloniaXamlLoader.Load(this);
        }

        private void AppTrayIcon_Clicked(object sender, EventArgs e) {
            return;
        }

        public override async void OnFrameworkInitializationCompleted() {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop) {
                string cefLogPath = Path.Combine(Environment.CurrentDirectory, "ceflog.txt");
                if(File.Exists(cefLogPath)) {
                    File.Delete(cefLogPath);
                }
                WebView.Settings.OsrEnabled = true;
                WebView.Settings.LogFile = "ceflog.txt";
                WebView.Settings.EnableErrorLogOnly = true;

                
                string cefCacheDir = Path.Combine(Environment.CurrentDirectory, "cefcache");
                if (!Directory.Exists(cefCacheDir)) {
                    Directory.CreateDirectory(cefCacheDir);
                }
                WebView.Settings.CachePath = cefCacheDir;


                await MpAvWrapper.Instance.InitializeAsync();
                await MpPlatformWrapper.InitAsync(MpAvWrapper.Instance);
                var bootstrapper = new MpAvBootstrapperViewModel();
                await bootstrapper.InitAsync();

                desktop.MainWindow = new MpAvMainWindow();
            }

            base.OnFrameworkInitializationCompleted();
        }

    }
}
